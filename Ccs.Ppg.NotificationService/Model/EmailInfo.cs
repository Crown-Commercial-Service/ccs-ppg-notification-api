using System.ComponentModel.DataAnnotations;

namespace Ccs.Ppg.NotificationService.Model
{
	public class EmailInfo
	{
		[Required(ErrorMessage = "Email is required Ex:UserName@example.com")]
		public string To { get; set; }

		[Required(ErrorMessage = "Email template is required")]
		public string TemplateId { get; set; }

		public Dictionary<string, dynamic> BodyContent { get; set; }
	}
}
