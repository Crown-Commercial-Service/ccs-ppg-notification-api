using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.AuthMethods;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ccs.Ppg.NotificationService.API.CustomOptions
{
  public class VaultConfigurationProvider : ConfigurationProvider
  {
    public VaultOptions _config;
    private IVaultClient _client;
    public VCapSettings _vcapSettings;

    public VaultConfigurationProvider(VaultOptions config)
    {
      _config = config;

      var env = System.Environment.GetEnvironmentVariable("VCAP_SERVICES", EnvironmentVariableTarget.Process);
      var vault = (JObject)JsonConvert.DeserializeObject<JObject>(env)["hashicorp-vault"][0];
      _vcapSettings = JsonConvert.DeserializeObject<VCapSettings>(vault.ToString());
      IAuthMethodInfo authMethod = new TokenAuthMethodInfo(vaultToken: _vcapSettings.credentials.auth.token);
      var vaultClientSettings = new VaultClientSettings(_vcapSettings.credentials.address, authMethod);
      _client = new VaultClient(vaultClientSettings);
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
      var mountPathValue = _vcapSettings.credentials.backends_shared.space.Split("/secret").FirstOrDefault();
      var _secrets = await _client.V1.Secrets.KeyValue.V1.ReadSecretAsync("secret/notification", mountPathValue);

      Data.Add("IsApiGatewayEnabled", _secrets.Data["IsApiGatewayEnabled"].ToString());
      Data.Add("ApiKey", _secrets.Data["ApiKey"].ToString());
      Data.Add("OrganisationApiUrl", _secrets.Data["OrganisationApiUrl"].ToString());

      if (_secrets.Data.ContainsKey("RedisCache"))
      {
        var redisCacheSettingsVault = JsonConvert.DeserializeObject<RedisCacheSettingsVault>(_secrets.Data["RedisCache"].ToString());
        Data.Add("RedisCache:IsEnabled", redisCacheSettingsVault.IsEnabled);
      }

      if (_secrets.Data.ContainsKey("Message"))
      {
        var messageSettingsVault = JsonConvert.DeserializeObject<MessageSettingsVault>(_secrets.Data["Message"].ToString());
        Data.Add("Message:ApiKey", messageSettingsVault.ApiKey);
        Data.Add("Message:TemplateId", messageSettingsVault.TemplateId);
      }

      if (_secrets.Data.ContainsKey("ConnectionStrings"))
      {
        var connectionStringsSettingsVault = JsonConvert.DeserializeObject<ConnectionStringsSettingsVault>(_secrets.Data["ConnectionStrings"].ToString());
        Data.Add("ConnectionStrings:CcsSso", connectionStringsSettingsVault.CcsSso);
      }

      if (_secrets.Data.ContainsKey("apis"))
      {
        var apisSettingsVault = JsonConvert.DeserializeObject<APIsSettingsVault>(_secrets.Data["apis"].ToString());
        Data.Add("apis:OrganisationUrl", apisSettingsVault.OrganisationUrl);
      }
    }
  }

  public class VaultConfigurationSource : IConfigurationSource
  {
    private VaultOptions _config;

    public VaultConfigurationSource(Action<VaultOptions> config)
    {
      _config = new VaultOptions();
      config.Invoke(_config);
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
      return new VaultConfigurationProvider(_config);
    }
  }

  public static class VaultExtensions
  {
    public static IConfigurationBuilder AddVault(this IConfigurationBuilder configuration,
    Action<VaultOptions> options)
    {
      var vaultOptions = new VaultConfigurationSource(options);
      configuration.Add(vaultOptions);
      return configuration;
    }
  }

  public class VaultOptions
  {
    public string Address { get; set; }
  }

  public class RedisCacheSettingsVault
  {
    public string IsEnabled { get; set; }
  }

  public class MessageSettingsVault
  {
    public string ApiKey { get; set; }
    public string TemplateId { get; set; }
  }

  public class ConnectionStringsSettingsVault
  {
    public string CcsSso { get; set; }
  }

  public class APIsSettingsVault
  {
    public string OrganisationUrl { get; set; }
  }
}
