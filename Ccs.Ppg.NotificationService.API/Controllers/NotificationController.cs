﻿using Ccs.Ppg.NotificationService.Model;
using Ccs.Ppg.NotificationService.Services.IServices;
using Ccs.Ppg.Utility.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace Ccs.Ppg.NotificationService.API.Controllers
{
  [Route("notification")]
  [ApiController]
  public class NotificationController : ControllerBase
  {
    private readonly IMessageProviderService _messageProviderService;
    private readonly IEmailProviderService _emailProviderService;
    private readonly IAwsSqsService _awsSqsService;
    public NotificationController(IMessageProviderService messageProviderService, IEmailProviderService emailProviderService,
      IAwsSqsService awsSqsService)
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
    public async Task SendEmail(EmailInfo emailInfo)
    {
      await _emailProviderService.SendEmailAsync(emailInfo);
    }

    [HttpPost("senduserconfirmemail")]
    [SwaggerOperation(Tags = new[] { "notification/senduserconfirmemail" })]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<bool> SendUserConfirmEmailOnlyUserIdPwd(object emailInfoRequest)
    {
      try
      {
        var emailInfo = JsonConvert.DeserializeObject<EmailInfo>(emailInfoRequest.ToString());
        await SendEmail(emailInfo);
        return true;
      }
      catch (Exception) 
      {
        await _awsSqsService.PushUserConfirmFailedEmailToDataQueueAsync(emailInfoRequest);
        return false;
      }
    }
  }
}
