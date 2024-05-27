using Ccs.Ppg.NotificationService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccs.Ppg.NotificationService.Tests.Infrastructure
{
  public class DtoHelper
  {
    public static EmailInfo GetEmailInfoRequest(string toMailId,string templateId, Dictionary<string, string> BodyContent)
    {
      return new EmailInfo
      {
        To = toMailId,
        TemplateId = templateId,
        BodyContent = BodyContent
      };
    }
    public static MessageRequestModel GetSMSInfoRequest(string mobileNumber, string templateId, List<MessageInfo> BodyContent)
    {
      return new MessageRequestModel
      {
        PhoneNumber = mobileNumber,
        TemplateId = templateId,
        Message = BodyContent
      };
    }
  }
}
