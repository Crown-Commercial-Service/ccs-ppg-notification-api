using  Ccs.Ppg.Utility.Cache.Config;
using  Ccs.Ppg.Utility.Cache.Repositories;
using  Ccs.Ppg.Utility.Cache.Services;
using  Ccs.Ppg.Utility.Cache.Services.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data;

namespace  Ccs.Ppg.Utility.Cache
{
    public static class Startup
    {
        public static void AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMemoryCache();
            services.AddSingleton(_ =>
                   new RedisConnectionPoolService(configuration["ConnectionStrings:Redis"])
               );
            services.AddTransient<IDbConnection>((sp) => new NpgsqlConnection(configuration["ConnectionStrings:CcsSso"]));

            services.AddSingleton<ICcsServiceRepository, CcsServiceRepository>();
            services.AddSingleton<ICcsServiceCacheService, CcsServiceCacheService>();
            services.AddSingleton<IMemoryCacheService, MemoryCacheService>();
            services.AddSingleton<IRedisCacheService, RedisCacheService>();
        }
    }
}
