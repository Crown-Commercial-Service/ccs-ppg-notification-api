using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccs.Ppg.NotificationService.Model
{
  public class EmailResponseInfo
  {
    public EmailInfo EmailInfo { get; set; }
    public bool IsUserInAuth0 { get; set; }
    public bool? isMessageRetry { get; set; }
  }
}
