using Ccs.Ppg.NotificationService.Model;
using Ccs.Ppg.Utility.Authorization;
using Ccs.Ppg.Utility.Cache;
using Ccs.Ppg.Utility.Exceptions;
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
        bool.TryParse(config["IsApiGatewayEnabled"], out bool isApiGatewayEnabled);
        bool.TryParse(config["EnableXRay"], out bool enableXray);
        bool.TryParse(config["RedisCache:IsEnabled"], out bool redisCacheEnabled);
        bool.TryParse(config["NotificationValidationConfigurations:EnableValidation"], out bool validationEnabled);
        int.TryParse(config["NotificationValidationConfigurations:SmsMsgLength"], out int smsMsgLength);
        int.TryParse(config["NotificationValidationConfigurations:OrgNameLegnth"], out int orgNameLegnth);
        int.TryParse(config["NotificationValidationConfigurations:FirstNameLength"], out int firstNameLength);
        int.TryParse(config["NotificationValidationConfigurations:LastNameLength"], out int LastNameLength);
        ApplicationConfigurationInfo appConfigInfo = new ApplicationConfigurationInfo()
        {
          IsApiGatewayEnabled = isApiGatewayEnabled,
          ApiKey = config["ApiKey"],
          OrganisationApiUrl = config["OrganisationApiUrl"],
          EnableXRay = enableXray,
          RedisCacheSettings = new RedisCacheSettings()
          {
            IsEnabled = redisCacheEnabled
          },
          MessageSettings = new MessageSettings()
          {
            ApiKey = config["Message:ApiKey"],
            TemplateId = config["Message:TemplateId"]
          },
          ConnectionStrings = new ConnectionStrings()
          {
            CcsSso = config["ConnectionStrings:CcsSso"]
          },
          EmailSettings = new EmailSettings()
          {
            ApiKey = config["Email:ApiKey"]
          },
          WrapperApiSettings = new WrapperApiSettings()
          {
            ApiGatewayDisabledConfigUrl = config["WrapperApiSettings:ApiGatewayDisabledConfigUrl"],
            ApiGatewayEnabledConfigUrl = config["WrapperApiSettings:ApiGatewayEnabledConfigUrl"],
            ConfigApiKey = config["WrapperApiSettings:ConfigApiKey"]
          },
          NotificationValidationConfigurations = new NotificationValidationConfigurations()
          {
            EnableValidation = validationEnabled,
            SmsMsgLength = smsMsgLength,
            OrgNameLegnth = orgNameLegnth,
            FirstNameLength = firstNameLength,
            LastNameLength = LastNameLength,
            EmailRegex = config["NotificationValidationConfigurations:EmailRegex"],
            LinkRegex = config["NotificationValidationConfigurations:LinkRegex"],
            CcsMsg = config["NotificationValidationConfigurations:CcsMsg"],
            SignInProviders = config.GetSection("NotificationValidationConfigurations:SignInProviders").Get<List<string>>(),
          }
        };
        return appConfigInfo;
      });

      // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
      services.AddEndpointsApiExplorer();
      var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

      var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
      services.AddSwagger(xmlPath, "Ccs.Ppg.NotificationService.API", "v1");
    }

    public static void ConfigurePipeline(this WebApplication app)
    {
      app.ConfigureSwagger();

      app.UseMiddleware<CommonExceptionHandlerMiddleware>();

      app.UseHttpsRedirection();

      app.ConfigureAuthorizationPipeline();

      app.MapControllers();
    }



  }
}
