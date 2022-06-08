using System;
using System.Threading.Tasks;

namespace  Ccs.Ppg.Utility.Cache.Services.IServices
{
    public interface IRedisCacheService
    {
        Task<TValue> Get<TValue>(string key);

        Task Set<TValue>(string key, TValue value);

        Task Set<TValue>(string key, TValue value, TimeSpan expiration);

        Task Remove(params string[] keys);
    }
}
