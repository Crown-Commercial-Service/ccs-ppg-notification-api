using System.Collections.Generic;
using System.Threading.Tasks;

namespace  Ccs.Ppg.Utility.Cache.Services
{
    public interface ICcsServiceCacheService
    {
        Task<IEnumerable<string>> GetServiceClients();
        Task<int?> GetDashBoardServiceId();
        Task<IEnumerable<string>> GetServiceClientIds(string ciiOrganisationId);
    }
}