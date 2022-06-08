using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  Ccs.Ppg.Utility.Authorization.Repositories
{
    public class OrganizationRepository : IOrganizationRepository
    {
        public string GetOrganizationId(string intendedOrganisationId)
        {
            //use dapper sql query to retrieve org id
            throw new NotImplementedException();
           //return  await _dataContext.User.Where(u => !u.IsDeleted && u.UserName == _requestContext.RequestIntendedUserName)
           //         .Select(u => u.Party.Person.Organisation.CiiOrganisationId).FirstOrDefaultAsync();
        }
    }
}
