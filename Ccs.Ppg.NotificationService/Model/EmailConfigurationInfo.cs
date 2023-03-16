namespace Ccs.Ppg.NotificationService.Model
{
  public class EmailConfigurationInfo
  {
    public string ApiKey { get; set; }

    public string UnverifiedUserDeletionNotificationTemplateId { get; set; }

    public string BulkUploadReportTemplateId { get; set; }

    public string UserRoleExpiredEmailTemplateId { get; set; }

  }
}
