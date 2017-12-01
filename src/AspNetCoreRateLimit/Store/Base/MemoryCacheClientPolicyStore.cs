using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace AspNetCoreRateLimit
{
    public class MemoryCachePolicyStore<T> : IPolicyStore<T> where T : RateLimitPolicies
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCachePolicyStore(IMemoryCache memoryCache, 
            IOptions<RateLimitOptions> options = null, 
            IOptions<RateLimitPolicies> policies = null)
        {
            _memoryCache = memoryCache;
            //save client rules defined in appsettings in cache on startup
            if(options != null && options.Value != null && policies != null && policies.Value != null && policies.Value.RuleSet != null)
            {
                Set($"{options.Value.PolicyPrefix}", (T)policies.Value);
                //foreach (var rule in policies.Value.RuleSet)
                //{
                //    //throw new NotImplementedException();
                //    //Set($"{options.Value.PolicyPrefix}_{rule.Id}", new RateLimitPolicy { Id = rule.Id, Rules = rule.Rules });
                //}
            }
        }

        public void Set(string id, T policy)
        {
            _memoryCache.Set(id, policy);
        }

        public bool Exists(string id)
        {
            RateLimitPolicy stored;
            return _memoryCache.TryGetValue(id, out stored);
        }

        public T Get(string id)
        {
            T stored;
            if (_memoryCache.TryGetValue(id, out stored))
            {
                return stored;
            }

            return null;
        }

        public void Remove(string id)
        {
            _memoryCache.Remove(id);
        }
    }
}
