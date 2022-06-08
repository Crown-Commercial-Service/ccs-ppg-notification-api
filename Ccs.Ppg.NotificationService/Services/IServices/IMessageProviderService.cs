using Ccs.Ppg.NotificationService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccs.Ppg.NotificationService.Services.IServices
{
    public interface IMessageProviderService
    {
        public Task<bool> SendMessage(MessageRequestModel messageInfo);
    }
}
