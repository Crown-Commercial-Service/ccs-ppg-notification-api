
namespace  Ccs.Ppg.Utility.Cache.Config
{
    public class RedisCacheConfig
    {
        public bool IsEnabled { get; set; }
        public int ExpirationTimeInMinutes { get; set; }
    }
}
