using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccs.Ppg.NotificationService.Model
{
  public class ApplicationConfigurationInfo
  {
    public bool IsApiGatewayEnabled { get; set; }
    public string ApiKey { get; set; }
    public string OrganisationApiUrl { get; set; }
    public bool EnableXRay { get; set; }
    public RedisCacheSettings RedisCacheSettings { get; set; }
    public MessageSettings MessageSettings { get; set; }
    public ConnectionStrings ConnectionStrings { get; set; }
    public EmailSettings EmailSettings { get; set; }
    public WrapperApiSettings WrapperApiSettings { get; set; }
    public NotificationValidationConfigurations NotificationValidationConfigurations { get; set; }
  }

  public class RedisCacheSettings
  {
    public bool IsEnabled { get; set; }
  }

  public class MessageSettings
  {
    public string ApiKey { get; set; }
    public string TemplateId { get; set; }
  }

  public class ConnectionStrings
  {
    public string CcsSso { get; set; }
  }

  public class EmailSettings
  {
    public string ApiKey { get; set; }
  }

  public class WrapperApiSettings
  {
    public string ConfigApiKey { get; set; }
    public string ApiGatewayEnabledConfigUrl { get; set; }
    public string ApiGatewayDisabledConfigUrl { get; set; }
  }

  public class NotificationValidationConfigurations
  {
    public bool EnableValidation { get; set; }
    public int SmsMsgLength { get; set; }
    public int OrgNameLegnth { get; set; }
    public string EmailRegex { get; set; }
    public int FirstNameLength { get; set; }
    public int LastNameLength { get; set; }
    public List<string> SignInProviders { get; set; }
    public string LinkRegex { get; set; }
    public string CcsMsg { get; set; }
  }
}
