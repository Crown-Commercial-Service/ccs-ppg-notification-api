using Microsoft.AspNetCore.Builder;

namespace  Ccs.Ppg.Utility.Authorization
{
    public static class SessionAuthorizationMiddlewareExtensions
    {
        public static IApplicationBuilder UseSessionAuthorization(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SessionAuthorizationMiddleware>();
        }
    }
}