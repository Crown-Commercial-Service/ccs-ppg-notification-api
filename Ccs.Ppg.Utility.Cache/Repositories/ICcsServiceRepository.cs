using System.Collections.Generic;
using System.Threading.Tasks;

namespace  Ccs.Ppg.Utility.Cache.Repositories
{
    public interface ICcsServiceRepository
    {
        Task<IEnumerable<string>> GetServiceClientIds();
        Task<int?> GetDashBoardServiceId();
        Task<IEnumerable<string>> GetServiceClientIds(string ciiOrganisationId);
    }
}