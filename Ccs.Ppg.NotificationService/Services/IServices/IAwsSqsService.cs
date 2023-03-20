using Ccs.Ppg.NotificationService.Model;

namespace Ccs.Ppg.NotificationService.Services.IServices
{
  public interface IAwsSqsService
  {
    Task SendMessageAsync(string queueUrl, string messageGroupId, SqsMessageDto sqsMessageDto);
  }
}
