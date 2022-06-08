namespace  Ccs.Ppg.Utility.Authorization.Services
{
    public interface IAuthService
    {
        bool AuthorizeUser(string[] claimList);
        Task<bool> AuthorizeForOrganisationAsync(RequestType requestType);
    }
}
