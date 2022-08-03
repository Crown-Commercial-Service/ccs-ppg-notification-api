namespace Ccs.Ppg.NotificationService.API.CustomOptions
{
  public class VCapSettings
  {
    public Credentials credentials { get; set; }
  }

  public class Credentials
  {
    public string address { get; set; }
    public Auth auth { get; set; }
    public Backend backends_shared { get; set; }
  }

  public class Auth
  {
    public string accessor { get; set; }
    public string token { get; set; }
  }

  public class Backend
  {
    public string application { get; set; }
    public string organization { get; set; }
    public string space { get; set; }
  }
}
