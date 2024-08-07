using Ccs.Ppg.NotificationService.Model;
using Ccs.Ppg.NotificationService.Services.IServices;
using Ccs.Ppg.Utility.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Ccs.Ppg.NotificationService.API.Controllers
{
    [Route("notification-service")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IMessageProviderService _messageProviderService;
		    private readonly IEmailProviderService _emailProviderService;		
		    public NotificationController(IMessageProviderService messageProviderService, IEmailProviderService emailProviderService)
        {
            _messageProviderService = messageProviderService;
			      _emailProviderService = emailProviderService;
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
        ///     POST /notification-service/sms
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
        [SwaggerOperation(Tags = new[] { "Notification Service - SMS" })]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<bool> Post(MessageRequestModel message)
        {
            return await _messageProviderService.SendMessage(message);
		    }

		    /// <summary>
		    /// Allows a user to send email
		    /// </summary>
		    /// <response  code="200">Ok</response>
		    /// <response  code="401">Unauthorised</response>
		    /// <response  code="403">Forbidden</response>
		    /// <response  code="404">Not found</response>
		    /// <response  code="400">Bad request. </response>
		    /// <remarks>
		    /// Sample request:
		    ///
		    ///     POST /notification-service/email
		    ///        {
		    ///        "to": "username@xxxx.com",
		    ///        "templateId": "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX",
		    ///        "bodyContent": {
		    ///                 "email": "UserName@xxxx.com",
		    ///                 "additionalProp3": "string",
		    ///                 "additionalProp3": "string"
        ///                 }
		    ///         }
		    ///
		    /// </remarks>

		    [HttpPost("email")]
		    [SwaggerOperation(Tags = new[] { "Notification Service - Email" })]
	      public async Task SendEmailAsync(EmailInfo emailInfo)
		    {
           await _emailProviderService.SendEmailAsync(emailInfo);
        }			      
		}
}