using Ccs.Ppg.NotificationService.Model;
using Ccs.Ppg.NotificationService.Services.IServices;
using Notify.Client;
using Notify.Models.Responses;
using System.Net;

namespace Ccs.Ppg.NotificationService.Services
{
  public class EmailProviderService : IEmailProviderService
  {
    private readonly EmailConfigurationInfo _emailConfigurationInfo;
    private readonly IHttpClientFactory _httpClientFactory;

    public EmailProviderService(EmailConfigurationInfo emailConfigurationInfo, IHttpClientFactory httpClientFactory)
    {
      _emailConfigurationInfo = emailConfigurationInfo;
      _httpClientFactory = httpClientFactory;
    }

    public async Task SendEmailAsync(EmailInfo emailInfo)
    {
      try
      {
        var client = _httpClientFactory.CreateClient();
        var httpClientWithProxy = new HttpClientWrapper(client);
        var notificationClient = new NotificationClient(httpClientWithProxy, _emailConfigurationInfo.ApiKey);
        EmailNotificationResponse response = await notificationClient.SendEmailAsync(emailInfo.To,
          emailInfo.TemplateId, emailInfo.BodyContent);
      }
      catch (Exception ex)
      {
        var wex = GetNestedException<WebException>(ex);

        // If there is no nested WebException, re-throw the exception.
        if (wex == null) { throw; }

        // Get the response object.
        var response = wex.Response as HttpWebResponse;

        // If it's not an HTTP response or is not error 403, re-throw.
        if (response == null || response.StatusCode != HttpStatusCode.Forbidden)
        {
          throw;
        }
      }
    }

    static T GetNestedException<T>(Exception ex) where T : Exception
    {
      if (ex == null) { return null; }

      var tEx = ex as T;
      if (tEx != null) { return tEx; }

      return GetNestedException<T>(ex.InnerException);
    }
  }
}
