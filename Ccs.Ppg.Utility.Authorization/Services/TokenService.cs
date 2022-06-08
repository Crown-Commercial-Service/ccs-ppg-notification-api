using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace  Ccs.Ppg.Utility.Authorization.Services
{
    public class JwtTokenValidationInfo
    {
        public bool IsValid { get; set; }

        public string Uid { get; set; }

        public string CiiOrgId { get; set; }

        public Dictionary<string, string> ClaimValues { get; set; }
    }

    public class TokenService : ITokenService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private JsonWebKey _jwk = null;

        public TokenService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<JwtTokenValidationInfo> ValidateTokenAsync(string token, IConfiguration config, List<string> claims = null)
        {
            var result = new JwtTokenValidationInfo();
            var issuer = config["JwtTokenSettings:Issuer"];
            _jwk = await GetJsonWebKey(issuer);
            var validationParameters = ValidationParameters(config, issuer);

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out _);
                result.IsValid = true;
                result.ClaimValues = Claims(token, claims, tokenHandler);
            }
            catch (Exception e)
            {
                result.IsValid = false;
            }
            return result;
        }

        public async Task<JwtTokenValidationInfo> ValidateTokenWithoutAudienceAsync(string token, IConfiguration config, List<string> claims = null)
        {
            var result = new JwtTokenValidationInfo();
            var issuer = config["JwtTokenSettings:Issuer"];
            _jwk = await GetJsonWebKey(issuer);

            var validationParameters = ValidationParameters(config, issuer);

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                //tokenHandler.ValidateToken(token, validationParameters, out _);
                result.IsValid = true;
                result.ClaimValues = Claims(token, claims, tokenHandler);
            }
            catch (Exception e)
            {
                result.IsValid = false;
            }
            return result;
        }
        private static Dictionary<string, string> Claims(string token, List<string> claims, JwtSecurityTokenHandler tokenHandler)
        {
            var jwtSecurityToken = tokenHandler.ReadJwtToken(token);
            Dictionary<string, string> resolvedClaims = new Dictionary<string, string>();
            if (claims != null)
            {
                foreach (var claim in claims.Where(c => c != "roles"))
                {
                    var claimValue = jwtSecurityToken.Claims.First(c => c.Type == claim).Value;
                    resolvedClaims.Add(claim, claimValue);
                }
                if (claims.Contains("roles"))
                {
                    var roleList = jwtSecurityToken.Claims.Where(c => c.Type == "roles").Select(c => c.Value);
                    resolvedClaims.Add("roles", string.Join(',', roleList));
                }
            }
            return resolvedClaims;
        }
        private TokenValidationParameters ValidationParameters(IConfiguration config, string issuer)
        {
            return new TokenValidationParameters
            {
                IssuerSigningKey = _jwk,
                ValidateIssuer = true,
                ValidateAudience = true,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ValidAudience = config["JwtTokenSettings:audience"],
                ValidIssuer = issuer,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero
            };
        }

        private async Task<JsonWebKey> GetJsonWebKey(string issuer)
        {
            if (_jwk == null)
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(issuer);
                var keys = await client.GetAsync("/security/.well-known/jwks.json");
                var jsonKeys = await keys.Content.ReadAsStringAsync();
                var jwks = JsonConvert.DeserializeObject<JsonWebKeySet>(jsonKeys);
                _jwk= jwks.Keys.First();
                return jwks.Keys.First();
            }
            else return _jwk;
        }

    }
}
