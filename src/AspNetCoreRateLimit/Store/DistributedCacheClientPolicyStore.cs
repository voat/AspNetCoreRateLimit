using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AspNetCoreRateLimit
{
    public class DistributedCacheClientPolicyStore : IClientPolicyStore
    {
        private readonly IDistributedCache _memoryCache;

        public DistributedCacheClientPolicyStore(IDistributedCache memoryCache, 
            IOptions<ClientRateLimitOptions> options = null, 
            IOptions<ClientRateLimitPolicies> policies = null)
        {
            _memoryCache = memoryCache;

            //save client rules defined in appsettings in distributed cache on startup
            if (options != null && options.Value != null && policies != null && policies.Value != null && policies.Value.RuleSet != null)
            {
                foreach (var rule in policies.Value.RuleSet)
                {
                    Set($"{options.Value.PolicyPrefix}_{rule.Id}", new RateLimitPolicy { Id = rule.Id, Rules = rule.Rules });
                }
            }
        }

        public void Set(string id, RateLimitPolicy policy)
        {
            _memoryCache.SetString(id, JsonConvert.SerializeObject(policy));
        }

        public bool Exists(string id)
        {
            var stored = _memoryCache.GetString(id);
            return !string.IsNullOrEmpty(stored);
        }

        public RateLimitPolicy Get(string id)
        {
            var stored = _memoryCache.GetString(id);
            if (!string.IsNullOrEmpty(stored))
            {
                return JsonConvert.DeserializeObject<RateLimitPolicy>(stored);
            }
            return null;
        }

        public void Remove(string id)
        {
            _memoryCache.Remove(id);
        }
    }
}
