using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ccs.Ppg.NotificationService.Model
{
  public class ServiceRoleGroup
  {
    public int Id { get; set; }

    [JsonPropertyOrder(-1)]
    public string Name { get; set; }

    [JsonPropertyOrder(-1)]
    public string Key { get; set; }

    public int DisplayOrder { get; set; }

    public string Description { get; set; }

    public int[] AutoValidationRoleTypeEligibility { get; set; } = { };

    public int ApprovalRequired { get; set; } = 0;
  }
}
