using Amazon.SimpleSystemsManagement.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ccs.Ppg.NotificationService.Services.IServices
{
  public interface IAwsParameterStoreService
  {
    Task<List<Parameter>> GetParameters(string path);

    string FindParameterByName(List<Parameter> parameters, string name);

    KeyValuePair<string, string> GetParameter(List<Parameter> parameters, string name, string key);

    List<KeyValuePair<string, string>> GetParameterFromCommaSeparated(List<Parameter> parameters, string name, string key);
  }
}
