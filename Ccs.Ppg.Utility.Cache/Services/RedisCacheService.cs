using  Ccs.Ppg.Utility.Cache.Config;
using  Ccs.Ppg.Utility.Cache.Services.IServices;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace  Ccs.Ppg.Utility.Cache.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly RedisCacheConfig _redisCacheConfig;
        private RedisConnectionPoolService _redisConnectionService;
        private string _baseKeyPath = string.Empty;

        public RedisCacheService(RedisConnectionPoolService redisConnectionService,
            IConfiguration config)
        {
            _redisConnectionService = redisConnectionService;
            _redisCacheConfig = config.GetSection("RedisCache").Get<RedisCacheConfig>();
        }

        public async Task<TValue> Get<TValue>(string key)
        {
            var value = await RedisDatabase.StringGetAsync(GetFullKey(key));
            return Deserialize<TValue>(value);
        }

        public async Task Set<TValue>(string key, TValue value)
        {
            await RedisDatabase.StringSetAsync(GetFullKey(key), Serialize(value));
        }

        public async Task Set<TValue>(string key, TValue value, TimeSpan expiration)
        {
            await RedisDatabase.StringSetAsync(GetFullKey(key), Serialize(value), expiration);
        }

        public async Task Remove(params string[] keys)
        {
            if (_redisCacheConfig.IsEnabled)
            {

                var tasks = new List<Task>();
                foreach (var key in keys)
                {
                    tasks.Add(RedisDatabase.KeyDeleteAsync(GetFullKey(key)));
                }

                await Task.WhenAll(tasks);
            }
        }

        public bool KeyExists(string key)
        {
            return RedisDatabase.KeyExists(key);
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            return await RedisDatabase.KeyExistsAsync(key);
        }

        // This should be moved to default interface implementation when C# 8.0 is available.
        // https://github.com/dotnet/csharplang/issues/288
        public async Task<TValue> GetOrSetValueAsync<TValue>(string key, Func<Task<TValue>> asyncResolver, int? expirationInMinutes = null)
        {
            var value = await Get<TValue>(key);
            if (value == null)
            {
                value = await asyncResolver();
                if (!expirationInMinutes.HasValue)
                {
                    await Set(key, value);
                }
                else
                {
                    await Set(key, value, new TimeSpan(0, expirationInMinutes.Value, 0));
                }
            }

            return value;
        }


        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        private string Serialize(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            // Using Json serialization for convenience + performance
            return JsonConvert.SerializeObject(obj, _jsonSettings);
        }

        private T Deserialize<T>(string str)
        {
            if (str == null)
            {
                return default(T);
            }

            // Using Json serialization for convenience + performance
            return JsonConvert.DeserializeObject<T>(str, _jsonSettings);
        }

        /// <summary>
        /// Returns the key with base path appended to it.
        /// </summary>
        private string GetFullKey(string key)
        {
            return $"{_baseKeyPath}/{key}";
        }

        private IDatabase RedisDatabase
        {
            get
            {
                return _redisConnectionService.GetDatabase();
            }
        }

    }
}
