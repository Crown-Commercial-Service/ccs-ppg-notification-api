using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Formatting.Compact;
using Microsoft.Extensions.DependencyInjection;

namespace Ccs.Ppg.Utility.Logging
{
    public static class Startup
    {
        public static void AddCustomLogging(this IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(new RenderedCompactJsonFormatter(), "C://Logs//logs.txt")
                .CreateLogger();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddSerilog(logger: Log.Logger);
            });
        }
        public static void UseLogging(this IApplicationBuilder app, IConfiguration config)
        {
            bool.TryParse(config["EnableAdditionalLogs"], out bool additionalLog);
            if (additionalLog)
            {
                app.UseMiddleware<RequestLogMiddleware>();
            }
        }
    }
}
