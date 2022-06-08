using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using  Ccs.Ppg.Utility.Authorization.Services;
using  Ccs.Ppg.Utility.Authorization.Repositories;

namespace  Ccs.Ppg.Utility.Authorization
{
    public static class StartUp
    {
        public static void AddAuthentication(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            services.AddScoped<RequestContext>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddSingleton<ITokenService, TokenService>();
            services.AddSingleton<IAuthorizationPolicyProvider, ClaimAuthorisationPolicyProvider>();
        }

        public static void ConfigureAuthorizationPipeline(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSessionAuthorization();
            app.UseOrganisationRequestContext();
        }
    }
}
