using Ccs.Ppg.NotificationService.Model;
using Ccs.Ppg.NotificationService.Services.IServices;
using Ccs.Ppg.Utility.Constants.Constants;
using Ccs.Ppg.Utility.Exceptions.Exceptions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Notify.Client;
using Notify.Models.Responses;

namespace Ccs.Ppg.NotificationService.Services
{
  public class MessageProviderService : IMessageProviderService
  {
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApplicationConfigurationInfo _applicationConfigurationInfo;

    public MessageProviderService(IHttpClientFactory httpClientFactory, ApplicationConfigurationInfo applicationConfigurationInfo)
    {
      _httpClientFactory = httpClientFactory;
      _applicationConfigurationInfo = applicationConfigurationInfo;
    }

    public async Task<bool> SendMessage(MessageRequestModel messageInfo)
    {
      try
      {
        // var data = new Dictionary<string, dynamic> { { "code", messageInfo.Message } };
        var data = new Dictionary<string, dynamic>();
        messageInfo.Message.ForEach(message => data.Add(message.key, message.Message));

        Console.WriteLine("SmsInfo Properties:");
        foreach (var d in data)
        {
          Console.WriteLine($"  {d.Key}: {d.Value}");
        }

        bool isValidationEnbled = _applicationConfigurationInfo.NotificationValidationConfigurations.EnableValidation;
        if (isValidationEnbled && !ValidateMessage(messageInfo.Message?.FirstOrDefault()))
        {
          Console.WriteLine(ErrorConstant.ErrorInvalidDetails);
          throw new CcsSsoException(ErrorConstant.ErrorInvalidDetails);
        }

        var apiKey = _applicationConfigurationInfo.MessageSettings.ApiKey;
        var templateId = !string.IsNullOrEmpty(messageInfo.TemplateId) ? messageInfo.TemplateId : _applicationConfigurationInfo.MessageSettings.TemplateId;

        var client = _httpClientFactory.CreateClient();
        var httpClientWithProxy = new HttpClientWrapper(client);
        var notificationClient = new NotificationClient(httpClientWithProxy, apiKey);

        SmsNotificationResponse response = notificationClient.SendSms(mobileNumber: messageInfo.PhoneNumber, templateId: templateId, personalisation: data);
        return true;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
        return false;
      }

    }

    public bool ValidateMessage(MessageInfo msgInfo)
    {
      var msgLength = _applicationConfigurationInfo.NotificationValidationConfigurations.SmsMsgLength;
      Console.WriteLine("Message OTP: " + msgInfo.Message);
      if (msgInfo != null && msgInfo.Message.Length > msgLength)
      {
        return false;
      }
      return true;
    }
  }
}
