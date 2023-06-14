using System.ComponentModel.DataAnnotations;

namespace Ccs.Ppg.NotificationService.Model
{
	public class EmailInfo
	{
		public string To { get; set; }

		public string TemplateId { get; set; }

		public Dictionary<string, dynamic> BodyContent { get; set; }
	}
}
