using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AspNetCoreRateLimit
{
    public class DistributedCachePolicyStore : IPolicyStore<RateLimitPolicies>
    {
        private readonly IDistributedCache _memoryCache;

        public DistributedCachePolicyStore(IDistributedCache memoryCache, 
            IOptions<RateLimitOptions> options = null, 
            IOptions<RateLimitPolicies> policies = null)
        {
            _memoryCache = memoryCache;

            //save ip rules defined in appsettings in distributed cache on startup
            if (options != null && options.Value != null && policies != null && policies.Value != null && policies.Value.RuleSet != null)
            {
                Set($"{options.Value.PolicyPrefix}", policies.Value);
            }
        }

        public void Set(string id, RateLimitPolicies policy)
        {
            _memoryCache.SetString(id, JsonConvert.SerializeObject(policy));
        }

        public bool Exists(string id)
        {
            var stored = _memoryCache.GetString(id);
            return !string.IsNullOrEmpty(stored);
        }

        public RateLimitPolicies Get(string id)
        {
            var stored = _memoryCache.GetString(id);
            if (!string.IsNullOrEmpty(stored))
            {
                return JsonConvert.DeserializeObject<RateLimitPolicies>(stored);
            }
            return null;
        }

        public void Remove(string id)
        {
            _memoryCache.Remove(id);
        }
    }
}
