using  Ccs.Ppg.Utility.Authorization.Repositories;
using Ccs.Ppg.Utility.Cache.Config;
using Ccs.Ppg.Utility.Cache.Services.IServices;
using Ccs.Ppg.Utility.Exceptions.Exceptions;
using Microsoft.Extensions.Configuration;

namespace  Ccs.Ppg.Utility.Authorization.Services
{
    public class AuthService : IAuthService
    {

        private readonly IRedisCacheService _redisCacheService;
        private readonly RequestContext _requestContext;
        private readonly RedisCacheConfig _redisCacheConfig;
        private readonly IOrganizationRepository _organizationRepository;

        public AuthService(RequestContext requestContext,
            IRedisCacheService redisCacheService,
            IConfiguration config,
            IOrganizationRepository organizationRepository)
        {
            _requestContext = requestContext;
            _redisCacheService = redisCacheService;
            _redisCacheConfig = config.GetSection("RedisCache").Get<RedisCacheConfig>();
            _organizationRepository = organizationRepository;
        }

        public async Task<bool> AuthorizeForOrganisationAsync(RequestType requestType)
        {
            var isCcsAdminRequest = _requestContext.Roles.Contains("ORG_USER_SUPPORT") || _requestContext.Roles.Contains("MANAGE_SUBSCRIPTIONS");
            var isAuthorizedForOrganisation = false;

            if (requestType == RequestType.HavingOrgId)
            {
                isAuthorizedForOrganisation = _requestContext.CiiOrganisationId == _requestContext.RequestIntendedOrganisationId;
            }
            else if (requestType == RequestType.NotHavingOrgId)
            {
                var intendedOrganisationId = await _redisCacheService.Get<string>($"{CacheKeyConstant.UserOrganisation}-{_requestContext.RequestIntendedUserName}");

                if (string.IsNullOrEmpty(intendedOrganisationId))
                {
                    intendedOrganisationId = _organizationRepository.GetOrganizationId(intendedOrganisationId);

                    await _redisCacheService.Set($"{CacheKeyConstant.UserOrganisation}-{_requestContext.RequestIntendedUserName}", intendedOrganisationId,
                      new TimeSpan(0, _redisCacheConfig.ExpirationTimeInMinutes, 0));
                }

                isAuthorizedForOrganisation = _requestContext.CiiOrganisationId == intendedOrganisationId;
            }

            if (!isAuthorizedForOrganisation && !isCcsAdminRequest)
            {
                throw new ForbiddenException();
            }

            return true;
        }

        public bool AuthorizeUser(string[] claimList)
        {
            var isAuthorized = _requestContext.Roles.Any(r => claimList.Any(c => r == c));

            // Org users (having only the ORG_DEFAULT_USER role) should not access other user details
            if (_requestContext.RequestType == RequestType.NotHavingOrgId && _requestContext.Roles.Count == 1 && _requestContext.Roles.Contains("ORG_DEFAULT_USER"))
            {
                isAuthorized = _requestContext.UserName == _requestContext.RequestIntendedUserName;
            }

            if (!isAuthorized)
            {
                throw new ForbiddenException();
            }

            return isAuthorized;
        }
    }
}
