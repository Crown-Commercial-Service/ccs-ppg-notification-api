using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  Ccs.Ppg.Utility.Cache.Repositories
{
    public class CcsServiceRepository: ICcsServiceRepository
    {
        private readonly IDbConnection _dbConnection;

        public CcsServiceRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<IEnumerable<string>> GetServiceClientIds()
        {
            const string sql = @"SELECT ""ServiceClientId""  FROM public.""CcsService"" WHERE ""ServiceName""='Dashboard Service'";

            // No need to use using statement. Dapper will automatically
            // open, close and dispose the connection for you.
            return await _dbConnection.QueryAsync<string>(sql);
        }

        public async Task<int?> GetDashBoardServiceId()
        {
            const string sql = @"SELECT ""Id""  FROM public.""CcsService"" WHERE ""ServiceName""='Dashboard Service'";

            // No need to use using statement. Dapper will automatically
            // open, close and dispose the connection for you.
            return (await _dbConnection.QueryAsync<int>(sql)).FirstOrDefault();
        }

        public async Task<IEnumerable<string>> GetServiceClientIds(string ciiOrganisationId)
        {
            string sql = $@"select ccss.""ServiceClientId"" from public.""Organisation"" org
                                inner join public.""OrganisationEligibleRole"" orgeli
                                 on org.""Id""=orgeli.""OrganisationId""
	                             inner join public.""CcsAccessRole"" ccsrole
                                 on ccsrole.""Id""=orgeli.""CcsAccessRoleId""
	                             inner join public.""ServiceRolePermission"" srp
                                 on srp.""CcsAccessRoleId"" = ccsrole.""Id""
	                             inner join public.""ServicePermission"" sp
                                 on sp.""Id"" = srp.""ServicePermissionId""
	                             inner join public.""CcsService"" ccss
                                 on ccss.""Id""= sp.""CcsServiceId""

	                            where orgeli.""IsDeleted"" =false
	                            and org.""CiiOrganisationId""={ciiOrganisationId}";

            // No need to use using statement. Dapper will automatically
            // open, close and dispose the connection for you.
            return await _dbConnection.QueryAsync<string>(sql);
        }

    }
}
