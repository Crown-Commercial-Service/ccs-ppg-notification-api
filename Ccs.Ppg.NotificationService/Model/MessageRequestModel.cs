using System.ComponentModel.DataAnnotations;

namespace Ccs.Ppg.NotificationService.Model
{
    public class MessageRequestModel
    {
        [Required(ErrorMessage ="Phone number is required Ex:+[country code][phone number]")]
        public string PhoneNumber { get; set; }
        
        [Required(ErrorMessage = "Message template is required. Template should match the Personalisation field")]
        public string TemplateId { get; set; }

        [Required(ErrorMessage = "Personalisation is required. Data format should match with the provided template")]
        public List<MessageInfo> Message { get; set; }
    }
}
