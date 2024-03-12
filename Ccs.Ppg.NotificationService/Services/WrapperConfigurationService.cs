using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Ccs.Ppg.NotificationService.Services.IServices;
using Ccs.Ppg.NotificationService.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Ccs.Ppg.NotificationService.Services
{
  public class WrapperConfigurationService : IWrapperConfigurationService
  {    
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApplicationConfigurationInfo _applicationConfigurationInfo;

    public WrapperConfigurationService(IHttpClientFactory httpClientFactory, ApplicationConfigurationInfo applicationConfigurationInfo)
    {      
      _httpClientFactory = httpClientFactory;
      _applicationConfigurationInfo = applicationConfigurationInfo;
    }

    public async Task<List<string>> GetServices()
    {
      List<string> services = new List<string>();
      var client = _httpClientFactory.CreateClient();
      client.BaseAddress = new Uri(_applicationConfigurationInfo.WrapperApiSettings.ApiGatewayEnabledConfigUrl);
      client.DefaultRequestHeaders.Add("X-API-Key", _applicationConfigurationInfo.WrapperApiSettings.ConfigApiKey);
      var response = await client.GetAsync("service-role-groups");
      var responseString = await response.Content.ReadAsStringAsync();

      if (response.IsSuccessStatusCode)
      {
        var result = JsonConvert.DeserializeObject<List<ServiceRoleGroup>>(responseString);
        services.AddRange(result.Select(x => x.Name));
      }
        return services;
    }
  }  
}
