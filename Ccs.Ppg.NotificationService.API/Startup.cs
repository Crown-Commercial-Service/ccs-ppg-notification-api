using Ccs.Ppg.Utility.Authorization;
using Ccs.Ppg.Utility.Cache;
using Ccs.Ppg.Utility.Logging;
using Ccs.Ppg.Utility.Swagger;
using System.Reflection;

namespace Ccs.Ppg.NotificationService.API
{
    public static class Startup
    {
        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddCustomLogging();
            services.AddHttpClient();
            services.AddHttpContextAccessor();

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            services.AddAuthentication(config);
            services.AddServices(config);
            services.AddControllers();
            services.AddRedis(config);

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            services.AddSwagger(xmlPath,"Ccs.Ppg.NotificationService.API","v1");
        }

        public static void ConfigurePipeline(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.ConfigureSwagger();
            }

            app.UseHttpsRedirection();

            app.ConfigureAuthorizationPipeline();

            app.MapControllers();
        }



    }
}
