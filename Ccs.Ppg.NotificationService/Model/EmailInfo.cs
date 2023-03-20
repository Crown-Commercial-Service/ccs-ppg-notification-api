namespace Ccs.Ppg.NotificationService.Model
{
  public class EmailInfoBase
  {
    public string To { get; set; }

    public string TemplateId { get; set; }
  }

  public class EmailInfoRequest
  {
    public string To { get; set; }

    public string TemplateId { get; set; }

    public string BodyContent { get; set; }
  }

  public class EmailInfo
  {
    public string To { get; set; }

    public string TemplateId { get; set; }

    public Dictionary<string, dynamic> BodyContent { get; set; }
  }
}
