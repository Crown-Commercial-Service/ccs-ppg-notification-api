using Microsoft.AspNetCore.Builder;

namespace  Ccs.Ppg.Utility.Authorization
{
    public static class RequestContextMiddlewareExtensions
    {
        public static IApplicationBuilder UseOrganisationRequestContext(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<OrganisationRequestContextFilterMiddleware>();
        }
    }
}