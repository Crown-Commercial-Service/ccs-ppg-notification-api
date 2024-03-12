using Ccs.Ppg.NotificationService.Model;
using Ccs.Ppg.NotificationService.Services.IServices;
using Ccs.Ppg.Utility.Constants.Constants;
using Ccs.Ppg.Utility.Exceptions.Exceptions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Notify.Client;
using Notify.Models.Responses;
using System.Text.RegularExpressions;

namespace Ccs.Ppg.NotificationService.Services
{
  public class EmailProviderService : IEmailProviderService
  {
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWrapperConfigurationService _wrapperConfigurationService;
    private readonly ApplicationConfigurationInfo _applicationConfigurationInfo;

    public EmailProviderService(IHttpClientFactory httpClientFactory, IWrapperConfigurationService wrapperConfigurationService,
      ApplicationConfigurationInfo applicationConfigurationInfo)
    {
      _httpClientFactory = httpClientFactory;
      _wrapperConfigurationService = wrapperConfigurationService;
      _applicationConfigurationInfo = applicationConfigurationInfo;
    }
    public async Task SendEmailAsync(EmailInfo emailInfo)
    {
      try
      {
        var bodyContent = new Dictionary<string, dynamic>();
        emailInfo.BodyContent.ToList().ForEach(pair => bodyContent.Add(pair.Key, pair.Value));
        bool isValidationEnbled = _applicationConfigurationInfo.NotificationValidationConfigurations.EnableValidation;
        if (isValidationEnbled && !await ValidateEmailMessage(bodyContent))
        {
          Console.WriteLine(ErrorConstant.ErrorInvalidDetails);          
          throw new CcsSsoException(ErrorConstant.ErrorInvalidDetails);
        }

        var apiKey = _applicationConfigurationInfo.EmailSettings.ApiKey;
        var client = _httpClientFactory.CreateClient();
        var httpClientWithProxy = new HttpClientWrapper(client);
        var notificationClient = new NotificationClient(httpClientWithProxy, apiKey);
        
        EmailNotificationResponse response = await notificationClient.SendEmailAsync(emailInfo.To, emailInfo.TemplateId, bodyContent);
      }
      catch (CcsSsoException ex)
      {
        throw ex;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
        throw new Exception(ex.Message);        
      }
    }

    public async Task<bool> ValidateEmailMessage(Dictionary<string, dynamic> msgBody)
    {
      foreach (var msg in msgBody)
      {
        switch (msg.Key.ToLower())
        {
          case "orgname":
            {
              var orgNameLength = _applicationConfigurationInfo.NotificationValidationConfigurations.OrgNameLegnth;
              if (!string.IsNullOrEmpty(msg.Value) && msg.Value.Length > orgNameLength)
                return false;
              break;
            }
          case "emailaddress":
          case "emailid":
            {
              var emailRegex = _applicationConfigurationInfo.NotificationValidationConfigurations.EmailRegex;
              Regex re = new Regex(emailRegex);
              if (!string.IsNullOrEmpty(msg.Value) && !re.IsMatch(msg.Value))
                return (false);
              break;
            }
          case "firstname":
            {
              var firstNameLength = _applicationConfigurationInfo.NotificationValidationConfigurations.FirstNameLength;
              if (!string.IsNullOrEmpty(msg.Value) && msg.Value.Length > firstNameLength)
                return false;
              break;
            }
          case "lastname":
            {
              var lastNameLength = _applicationConfigurationInfo.NotificationValidationConfigurations.LastNameLength;
              if (!string.IsNullOrEmpty(msg.Value) && msg.Value.Length > lastNameLength)
                return false;
              break;
            }
          case "link":
          case "mfaresetlink":
          case "federatedlogin":
            {
              var linkRegex = _applicationConfigurationInfo.NotificationValidationConfigurations.LinkRegex;
              Regex re = new Regex(linkRegex);
              if (!string.IsNullOrEmpty(msg.Value) && !re.IsMatch(msg.Value))
                return (false);
              break;
            }
          case "servicenames":
            {
              if (!string.IsNullOrEmpty(msg.Value))
              {
                var selectedServices = msg.Value.Split(',');
                List<string> delegatedServices = new List<string>();
                foreach (var service in selectedServices)
                {
                  delegatedServices.Add(service.Trim());
                }
                var services = await _wrapperConfigurationService.GetServices();
                var result = delegatedServices.Except(services).ToList();
                if (result.Count > 0) return false;
              }
              break;
            }
          case "sigininproviders":
            {
              if (!string.IsNullOrEmpty(msg.Value))
              {
                //validate against list of signin providers              
                var signinProviders = _applicationConfigurationInfo.NotificationValidationConfigurations.SignInProviders;
                var selectedProviders = msg.Value.Split(',');
                List<string> providers = new List<string>();
                providers.AddRange(selectedProviders);
                var result = providers.Except(signinProviders).ToList();
                if (result.Count > 0) return false;
              }
              break;
            }
          case "ccsmsg":
            {
              string ccsMsg = _applicationConfigurationInfo.NotificationValidationConfigurations.CcsMsg;
              if (!string.IsNullOrEmpty(msg.Value) && msg.Value != ccsMsg)
                return false;
              break;
            }

        }
      }
      return true;
    }
  }
}


