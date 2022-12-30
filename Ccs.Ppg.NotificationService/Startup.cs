using Ccs.Ppg.NotificationService.Services;
using Ccs.Ppg.NotificationService.Services.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccs.Ppg.NotificationService
{
  public static class Startup
  {
    public static void AddServices(this IServiceCollection services, IConfiguration config)
    {
      services.AddScoped<IMessageProviderService, MessageProviderService>();
      services.AddScoped<IAwsParameterStoreService, AwsParameterStoreService>();
    }
  }
}
