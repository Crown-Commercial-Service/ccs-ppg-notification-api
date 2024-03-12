namespace Ccs.Ppg.NotificationService.Services.IServices
{
  public interface IWrapperConfigurationService
  {
    Task<List<string>> GetServices();
  }
}
