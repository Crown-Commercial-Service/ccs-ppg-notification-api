using Microsoft.Extensions.Configuration;

namespace  Ccs.Ppg.Utility.Authorization.Services
{
    public interface ITokenService
    {
        Task<JwtTokenValidationInfo> ValidateTokenAsync(string token, IConfiguration config, List<string> claims = null);
    }
}
