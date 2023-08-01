using Ccs.Ppg.NotificationService.Model;
using Ccs.Ppg.NotificationService.Services.IServices;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Notify.Client;
using Notify.Models.Responses;

namespace Ccs.Ppg.NotificationService.Services
{
   public class EmailProviderService : IEmailProviderService
	{
		private readonly IConfiguration _configuration;
		private readonly IHttpClientFactory _httpClientFactory;

		public EmailProviderService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
		{
			_configuration = configuration;
			_httpClientFactory = httpClientFactory;
		}
		public async Task SendEmailAsync(EmailInfo emailInfo)
		{
			try
			{ 
			var apiKey = _configuration["Email:ApiKey"];
			
			var client = _httpClientFactory.CreateClient();
			var httpClientWithProxy = new HttpClientWrapper(client);
				var notificationClient = new NotificationClient(httpClientWithProxy, apiKey);
				var bodyContent = new Dictionary<string, dynamic>();
				emailInfo.BodyContent.ToList().ForEach(pair => bodyContent.Add(pair.Key, pair.Value));
        EmailNotificationResponse response = await notificationClient.SendEmailAsync(emailInfo.To, emailInfo.TemplateId, bodyContent);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				throw new Exception(ex.Message);
			}
		}
	}
}
  

