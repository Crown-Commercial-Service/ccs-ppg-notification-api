using Ccs.Ppg.NotificationService.API.CustomOptions;
using Ccs.Ppg.NotificationService.Model;
using Ccs.Ppg.NotificationService.Services.IServices;
using Ccs.Ppg.Utility.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Annotations;
using static Dapper.SqlMapper;

namespace Ccs.Ppg.NotificationService.API.Controllers
{
  [Route("notification")]
  [ApiController]
  public class NotificationController : ControllerBase
  {
    private readonly IMessageProviderService _messageProviderService;
    private readonly IEmailProviderService _emailProviderService;
    private readonly IAwsSqsService _awsSqsService;

    public NotificationController(IMessageProviderService messageProviderService, IEmailProviderService emailProviderService, IAwsSqsService awsSqsService)
    {
      _messageProviderService = messageProviderService;
      _emailProviderService = emailProviderService;
      _awsSqsService = awsSqsService;
    }

    /// <summary>
    /// Allows a user to send SMS
    /// </summary>
    /// <response  code="200">Ok</response>
    /// <response  code="401">Unauthorised</response>
    /// <response  code="403">Forbidden</response>
    /// <response  code="404">Not found</response>
    /// <response  code="400">Bad request. </response>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /notification/sms
    ///     {
    ///        "phoneNumber": +44123456,
    ///        "templateId": "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX",
    ///        "personalisation": {
    //                 {"key": "code", "message":"message"}
    ///        }
    ///     }
    ///
    /// </remarks>

    [HttpPost("sms")]
    [SwaggerOperation(Tags = new[] { "notification/sms" })]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<bool> Post(MessageRequestModel message)
    {
      return await _messageProviderService.SendMessage(message);
    }

    [HttpPost("email")]
    [SwaggerOperation(Tags = new[] { "notification/email" })]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<bool> SendEmail(EmailInfo emailInfo)
    {
      await _emailProviderService.SendEmailAsync(emailInfo);
      return true;
    }

    [HttpPost("senduserconfirmemail")]
    [SwaggerOperation(Tags = new[] { "notification/senduserconfirmemail" })]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<bool> SendUserConfirmEmailOnlyUserIdPwd(object emailInfoRequest)
    {
      var emailRequest = JsonConvert.DeserializeObject<EmailResponseInfo>(emailInfoRequest.ToString());
      try
      {

        if (!emailRequest.IsUserInAuth0)
        {
          Console.WriteLine($"RateLimitCheck: user doesn't exists inauth 0. Email- {emailRequest.EmailInfo.To}");
          Console.WriteLine($"RateLimitCheck: Adding message to the queue. Email- {emailRequest.EmailInfo.To}");


          object queueBody = new EmailResponseInfo { EmailInfo = emailRequest.EmailInfo, IsUserInAuth0 = true };

          var data = JsonConvert.SerializeObject(queueBody);

          await _awsSqsService.PushUserConfirmFailedEmailToDataQueueAsync(data);
        }
        else
        {
          Console.WriteLine($"RateLimitCheck: user exists inauth 0. Email- {emailRequest.EmailInfo.To}");

          var emailInfo = emailRequest.EmailInfo;
          var acticationLink = await _emailProviderService.GetActivationEmailVerificationLink(emailInfo.To);
          if (acticationLink != null)
            emailInfo.BodyContent["link"] = acticationLink;
          await SendEmail(emailInfo);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"RateLimitCheck: Exception raised. Email  {emailRequest.EmailInfo.To}");

        if (ex.Message == "ERROR_IDAM_REGISTRATION_FAILED" || ex.Message.Contains("Your system clock must be accurate to within 30 seconds"))
        {
          Console.WriteLine($"RateLimitCheck: Exception -ERROR_IDAM_REGISTRATION_FAILED.  Email  {emailRequest.EmailInfo.To}");

          if (emailRequest.isMessageRetry == null || emailRequest.isMessageRetry == false)
          {
            Console.WriteLine($"RateLimitCheck: adding message to queue. Email  {emailRequest.EmailInfo.To}");

            await _awsSqsService.PushUserConfirmFailedEmailToDataQueueAsync(emailInfoRequest);
          }else
          {
            throw;
          }
        }
        else
        {
          return false;
        }
      }

      return true;
    }
  }
}