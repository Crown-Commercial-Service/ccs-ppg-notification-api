using Ccs.Ppg.NotificationService.Services;
using Ccs.Ppg.NotificationService.Services.IServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ccs.Ppg.NotificationService.API.CustomOptions
{
  public class ParameterStoreConfigurationProvider : ConfigurationProvider
  {
    private string path = "/conclave-sso/notification/";
    private IAwsParameterStoreService _awsParameterStoreService;

    public ParameterStoreConfigurationProvider()
    {
      _awsParameterStoreService = new AwsParameterStoreService();
    }

    public override void Load()
    {
      LoadAsync().Wait();
    }

    public async Task LoadAsync()
    {
      await GetSecrets();
    }

    public async Task GetSecrets()
    {
      var parameters = await _awsParameterStoreService.GetParameters(path);
      var configurations = new List<KeyValuePair<string, string>>();

      configurations.Add(_awsParameterStoreService.GetParameter(parameters, path + "IsApiGatewayEnabled", "IsApiGatewayEnabled"));
      configurations.Add(_awsParameterStoreService.GetParameter(parameters, path + "ApiKey", "ApiKey"));
      configurations.Add(_awsParameterStoreService.GetParameter(parameters, path + "OrganisationApiUrl", "OrganisationApiUrl"));

      configurations.Add(_awsParameterStoreService.GetParameter(parameters, path + "RedisCache/IsEnabled", "RedisCache:IsEnabled"));

      configurations.Add(_awsParameterStoreService.GetParameter(parameters, path + "Message/ApiKey", "Message:ApiKey"));
      configurations.Add(_awsParameterStoreService.GetParameter(parameters, path + "Message/TemplateId", "Message:TemplateId"));

      var dbName = _awsParameterStoreService.FindParameterByName(parameters, path + "ConnectionStrings/Name");
      var dbConnection = _awsParameterStoreService.FindParameterByName(parameters, path + "ConnectionStrings/CcsSso");

      if (!string.IsNullOrEmpty(dbName))
      {
        var dynamicDBConnection = GetDatbaseConnectionString(dbName, dbConnection);
        Data.Add("ConnectionStrings:CcsSso", dynamicDBConnection);
      }
      else
      {
        Data.Add("ConnectionStrings:CcsSso", dbConnection);
      }

      configurations.Add(_awsParameterStoreService.GetParameter(parameters, path + "apis/OrganisationUrl", "apis:OrganisationUrl"));

      foreach (var configuration in configurations)
      {
        Data.Add(configuration);
      }
    }

    public static string GetDatbaseConnectionString(string name, string connectionString)
    {
      string env = Environment.GetEnvironmentVariable("VCAP_SERVICES", EnvironmentVariableTarget.Process);
      var envData = (JObject)JsonConvert.DeserializeObject(env);
      string setting = JsonConvert.SerializeObject(envData["postgres"].FirstOrDefault(obj => obj["name"].Value<string>() == name));
      var postgresSettings = JsonConvert.DeserializeObject<PostgresSettings>(setting.ToString());

      connectionString = connectionString.Replace("[Server]", postgresSettings.credentials.host);
      connectionString = connectionString.Replace("[Port]", postgresSettings.credentials.port);
      connectionString = connectionString.Replace("[Database]", postgresSettings.credentials.name);
      connectionString = connectionString.Replace("[Username]", postgresSettings.credentials.username);
      connectionString = connectionString.Replace("[Password]", postgresSettings.credentials.password);

      return connectionString;
    }
  }

  public class ParameterStoreConfigurationSource : IConfigurationSource
  {
    public ParameterStoreConfigurationSource()
    {
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
      return new ParameterStoreConfigurationProvider();
    }
  }

  public static class ParameterStoreExtensions
  {
    public static IConfigurationBuilder AddParameterStore(this IConfigurationBuilder configuration)
    {
      var parameterStoreConfigurationSource = new ParameterStoreConfigurationSource();
      configuration.Add(parameterStoreConfigurationSource);
      return configuration;
    }
  }

  public class PostgresSettings
  {
    public PostgresCredentials credentials { get; set; }
  }

  public class PostgresCredentials
  {
    public string host { get; set; }
    public string name { get; set; }
    public string password { get; set; }
    public string port { get; set; }
    public string username { get; set; }
  }  
}
