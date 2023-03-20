using Ccs.Ppg.NotificationService.Model;
using Ccs.Ppg.NotificationService.Services.IServices;
using Ccs.Ppg.Utility.Exceptions.Exceptions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Notify.Client;
using Notify.Models.Responses;
using System.Net;
using System.Web;

namespace Ccs.Ppg.NotificationService.Services
{
  public class EmailProviderService : IEmailProviderService
  {
    private readonly EmailConfigurationInfo _emailConfigurationInfo;
    private readonly SecurityApiSetting _securityApiSetting;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAwsSqsService _awsSqsService;
    private readonly IConfiguration _configuration;

    public EmailProviderService(EmailConfigurationInfo emailConfigurationInfo, IHttpClientFactory httpClientFactory, 
      IAwsSqsService awsSqsService, IConfiguration configuration, SecurityApiSetting securityApiSetting)
    {
      _emailConfigurationInfo = emailConfigurationInfo;
      _httpClientFactory = httpClientFactory;
      _awsSqsService = awsSqsService;
      _configuration = configuration;
      _securityApiSetting = securityApiSetting;
    }

    public async Task SendEmailAsync(EmailInfo emailInfo)
    {
      try
      {
        //throw new CcsSsoException("ERROR_IDAM_REGISTRATION_FAILED");
        var client = _httpClientFactory.CreateClient();
        var httpClientWithProxy = new HttpClientWrapper(client);
        var notificationClient = new NotificationClient(httpClientWithProxy, _emailConfigurationInfo.ApiKey);
        EmailNotificationResponse response = await notificationClient.SendEmailAsync(emailInfo.To,
          emailInfo.TemplateId, emailInfo.BodyContent);
      }
      catch (Exception)
      {
        throw;
      }
    }

    public async Task<string> GetActivationEmailVerificationLink(string email)
    {
      var client = _httpClientFactory.CreateClient();
      client.BaseAddress = new Uri(_securityApiSetting.Url);
      client.DefaultRequestHeaders.Add("X-API-Key", _securityApiSetting.ApiKey);

      var url = "security/users/activation-email-verification-link?email=" + HttpUtility.UrlEncode(email);
      var response = await client.GetAsync(url);

      if (!response.IsSuccessStatusCode)
        {
        throw new CcsSsoException("ERROR_IDAM_REGISTRATION_FAILED");
      }

      return await response.Content.ReadAsStringAsync();
    }


  }
}
