namespace Ccs.Ppg.NotificationService.Model
{
  public class SqsMessageDto
  {
    public string MessageBody { get; set; }

    public Dictionary<string, string> StringCustomAttributes { get; set; }

    public Dictionary<string, int> NumberCustomAttributes { get; set; }
  }
}
