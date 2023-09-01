using Ccs.Ppg.Utility.Authorization.Services;
using Ccs.Ppg.Utility.Cache.Services;
using Ccs.Ppg.Utility.Cache.Services.IServices;
using Ccs.Ppg.Utility.Constants.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace Ccs.Ppg.Utility.Authorization
{
    public class SessionAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IConfiguration _config;
        private readonly ITokenService _tokenService;
        private readonly ICcsServiceCacheService _ccsServiceCacheService;


        public SessionAuthorizationMiddleware(RequestDelegate next,
            IRedisCacheService redisCacheService,
            IConfiguration config,
            ITokenService tokenService,
            ICcsServiceCacheService ccsServiceCacheService)
        {
            _next = next;
            _redisCacheService = redisCacheService;
            _config = config;
            _tokenService = tokenService;
            _ccsServiceCacheService = ccsServiceCacheService;
        }

        public async Task Invoke(HttpContext context, RequestContext requestContext)
        {
            var success = await ValidateTokenOrApiKey(context, requestContext);
            if (success)
                await _next(context);
        }

        private async Task<bool> ValidateTokenOrApiKey(HttpContext context, RequestContext requestContext)
        {
            requestContext.IpAddress = context.GetRemoteIPAddress();
            requestContext.Device = context.Request.Headers["User-Agent"];
            var apiKey = context.Request.Headers["X-API-Key"];
            var bearerToken = context.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(bearerToken) && (string.IsNullOrEmpty(apiKey) || apiKey != _config["ApiKey"]))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return false;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(bearerToken))
                {
                    await ValidateToken(context, bearerToken, requestContext);
                }
            }
            return true;
        }

        private async Task<bool> ValidateToken(HttpContext context, string? bearerToken, RequestContext requestContext)
        {
            var token = bearerToken.Split(' ').Last();
            var result = await _tokenService.ValidateTokenAsync(token, _config,
              new List<string>() { "uid", "ciiOrgId", "sub", JwtRegisteredClaimNames.Jti, JwtRegisteredClaimNames.Exp, "roles", "caller", "sid" });

            if (result.IsValid)
            {
                await ValidateSession(context);
                await PopulateRequestContext(requestContext, result);
            }
            else
            {
                throw new UnauthorizedAccessException();
            }
            return true;
        }

        private async Task PopulateRequestContext(RequestContext requestContext, JwtTokenValidationInfo result)
        {
            if (result.ClaimValues["caller"] == "service")
            {
                requestContext.ServiceClientId = result.ClaimValues["sub"];
                requestContext.ServiceId = int.Parse(result.ClaimValues["uid"]);
            }
            else
            {
                requestContext.UserId = int.Parse(result.ClaimValues["uid"]);
                requestContext.UserName = result.ClaimValues["sub"];

                var serviceId = await _ccsServiceCacheService.GetDashBoardServiceId();
                if (serviceId != null)
                    requestContext.ServiceId = (int)serviceId;
            }

            requestContext.CiiOrganisationId = result.ClaimValues["ciiOrgId"];
            requestContext.Roles = result.ClaimValues["roles"].Split(",").ToList();
        }

        private async Task ValidateSession(HttpContext context)
        {
            var sub = context.User.Claims.First(c => c.Type == "sub")?.Value;
            var jti = context.User.Claims.First(c => c.Type == "jti")?.Value;
            var sessionId = context.User.Claims.First(c => c.Type == "sid")?.Value;

            var isInvalidSession = await _redisCacheService.Get<bool>(sessionId);
            Console.WriteLine($"SessionId : {sessionId} **==** invalid: {isInvalidSession}");
            if (isInvalidSession) //if session was invalidated due to logout from other clients
            {
                throw new UnauthorizedAccessException();
            }

            var forceSignout = await _redisCacheService.Get<bool>(CacheKeyConstant.ForceSignoutKey + sub);
            if (forceSignout) //check if user is entitled to force signout
            {
                throw new UnauthorizedAccessException();
            }
            else
            {
                var value = await _redisCacheService.Get<string>(CacheKeyConstant.BlockedListKey + jti);
                if (!string.IsNullOrEmpty(value))
                {
                    //Should terminate surving
                    throw new UnauthorizedAccessException();
                }
            }
        }
    }
}