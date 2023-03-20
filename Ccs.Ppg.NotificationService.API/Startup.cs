using Ccs.Ppg.NotificationService.Model;
using Ccs.Ppg.NotificationService.Services.IServices;
using Ccs.Ppg.NotificationService.Services;
using Ccs.Ppg.Utility.Authorization;
using Ccs.Ppg.Utility.Cache;
using Ccs.Ppg.Utility.Logging;
using Ccs.Ppg.Utility.Swagger;
using System.Reflection;

namespace Ccs.Ppg.NotificationService.API
{
  public static class Startup
  {
    public static void ConfigureServices(this IServiceCollection services, ConfigurationManager config)
    {
      services.AddCustomLogging();
      services.AddHttpClient();
      services.AddHttpContextAccessor();

      services.AddAuthentication(config);
      services.AddServices(config);
      services.AddControllers();
      services.AddRedis(config);

      services.AddSingleton(s =>
      {
        EmailConfigurationInfo emailConfigurationInfo = new()
        {
          ApiKey = config["Email:ApiKey"],
        };

        return emailConfigurationInfo;
      });

      services.AddSingleton(s =>
      {
        SecurityApiSetting securityApiSetting = new()
        {
          ApiKey = config["SecurityApiSettings:ApiKey"],
          Url = config["SecurityApiSettings:Url"]
        };

        return securityApiSetting;
      });

      services.AddSingleton(s =>
      {
        int.TryParse(config["QueueInfo:DataQueueRecieveMessagesMaxCount"], out int dataQueueRecieveMessagesMaxCount);
        dataQueueRecieveMessagesMaxCount = dataQueueRecieveMessagesMaxCount == 0 ? 10 : dataQueueRecieveMessagesMaxCount;

        int.TryParse(config["QueueInfo:DataQueueRecieveWaitTimeInSeconds"], out int dataQueueRecieveWaitTimeInSeconds); // Default value 0

        var sqsConfiguration = new SqsConfiguration
        {
          ServiceUrl = config["QueueInfo:ServiceUrl"],
          AccessKeyId = config["QueueInfo:DataQueueAccessKeyId"],
          AccessSecretKey = config["QueueInfo:DataQueueAccessSecretKey"],
          DataQueueRecieveMessagesMaxCount = dataQueueRecieveMessagesMaxCount,
          DataQueueRecieveWaitTimeInSeconds = dataQueueRecieveWaitTimeInSeconds
        };

        return sqsConfiguration;
      });

      services.AddSingleton<IAwsSqsService, AwsSqsService>();
      services.AddSingleton<IEmailProviderService, EmailProviderService>();


      // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
      services.AddEndpointsApiExplorer();
      var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

      var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
      services.AddSwagger(xmlPath, "Ccs.Ppg.NotificationService.API", "v1");
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
