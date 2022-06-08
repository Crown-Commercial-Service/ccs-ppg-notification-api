using Ccs.Ppg.NotificationService.Model;
using Ccs.Ppg.NotificationService.Services.IServices;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Notify.Client;
using Notify.Models.Responses;

namespace Ccs.Ppg.NotificationService.Services
{
  public class MessageProviderService : IMessageProviderService
  {
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public MessageProviderService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
      _httpClientFactory = httpClientFactory;
      _configuration = configuration;
    }

    public async Task<bool> SendMessage(MessageRequestModel messageInfo)
    {
      try
      {
        var apiKey = _configuration["Message:ApiKey"];
        var templateId = !string.IsNullOrEmpty(messageInfo.TemplateId) ? messageInfo.TemplateId : _configuration["Message:TemplateId"];

        var client = _httpClientFactory.CreateClient();
        var httpClientWithProxy = new HttpClientWrapper(client);
        var notificationClient = new NotificationClient(httpClientWithProxy, apiKey);

        // var data = new Dictionary<string, dynamic> { { "code", messageInfo.Message } };
        var data = new Dictionary<string, dynamic>();
        messageInfo.Message.ForEach(message => data.Add(message.key, message.Message));

        SmsNotificationResponse response = notificationClient.SendSms(mobileNumber: messageInfo.PhoneNumber, templateId: templateId, personalisation: data);
        return true;

      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
        return false;
      }

    }
  }
}
