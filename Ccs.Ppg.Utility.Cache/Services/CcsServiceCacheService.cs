using  Ccs.Ppg.Utility.Cache.Config;
using  Ccs.Ppg.Utility.Cache.Repositories;
using  Ccs.Ppg.Utility.Cache.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Ccs.Ppg.Utility.Constants.Constants;

namespace  Ccs.Ppg.Utility.Cache.Services
{
    public class CcsServiceCacheService: ICcsServiceCacheService
    {
        private readonly IMemoryCacheService _localCacheService;
        private readonly ICcsServiceRepository _serviceClientRepository;

        private readonly MemoryCacheConfig _memoryCacheConfig;
        public CcsServiceCacheService(IMemoryCacheService localCacheService, 
            ICcsServiceRepository serviceClientRepository,
            IConfiguration config)
        {
            _localCacheService = localCacheService;
            _serviceClientRepository = serviceClientRepository;
            _memoryCacheConfig = config.GetSection("MemoryCache").Get<MemoryCacheConfig>();
        }

        public async Task<IEnumerable<string>> GetServiceClients()
        {
            var serviceClientIds = _localCacheService.GetValue<IEnumerable<string>>("SERVICE_CLIENT_IDS");
            if (serviceClientIds == null || !serviceClientIds.Any())
            {
                serviceClientIds =await _serviceClientRepository.GetServiceClientIds();

                _localCacheService.SetValue("SERVICE_CLIENT_IDS", serviceClientIds, new TimeSpan(0, _memoryCacheConfig.ExpirationTimeInMinutes, 0));
            }
            return serviceClientIds;
        } 
        
        public async Task<int?> GetDashBoardServiceId()
        {
            var serviceId =  _localCacheService.GetValue<int?>(CacheKeys.DashboardServiceId);

            if (serviceId == null)
            {
                serviceId = await _serviceClientRepository.GetDashBoardServiceId();

                _localCacheService.SetValue(CacheKeys.DashboardServiceId, serviceId, new TimeSpan(0, _memoryCacheConfig.ExpirationTimeInMinutes, 0));
            }
            return serviceId;
        }

        public async Task<IEnumerable<string>> GetServiceClientIds(string ciiOrganisationId)
        {
            var serviceClientIds = _localCacheService.GetValue<IEnumerable<string>>($"ORGANISATION_SERVICE_CLIENT_IDS-{ciiOrganisationId}");
            if (serviceClientIds == null || !serviceClientIds.Any())
            {
                serviceClientIds = await _serviceClientRepository.GetServiceClientIds(ciiOrganisationId);

                _localCacheService.SetValue($"ORGANISATION_SERVICE_CLIENT_IDS-{ciiOrganisationId}", serviceClientIds, new TimeSpan(0, _memoryCacheConfig.ExpirationTimeInMinutes, 0));
            }
            return serviceClientIds;
        }
    }
}
