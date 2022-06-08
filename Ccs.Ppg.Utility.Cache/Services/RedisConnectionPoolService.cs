using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace  Ccs.Ppg.Utility.Cache.Services
{
  public class RedisConnectionPoolService
  {
    private Lazy<ConnectionMultiplexer> lazyConnection;
    private Lazy<IDatabase> lazyDatabase;

    public RedisConnectionPoolService(string redisCacheConnectionString)
    {
      SetupConnectionMultiplexer(redisCacheConnectionString);
    }

    private void SetupConnectionMultiplexer(string connectionString)
    {
      lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
      {
        var options = ConfigurationOptions.Parse(connectionString);
        return ConnectionMultiplexer.Connect(options);
      });

      lazyDatabase = new Lazy<IDatabase>(() =>
          lazyConnection.Value.GetDatabase());
    }

    public IDatabase GetDatabase()
    {
      return lazyDatabase.Value;
    }
  }
}
