﻿using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace AspNetCoreRateLimit
{
    public class MemoryCacheClientPolicyStore : IClientPolicyStore
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheClientPolicyStore(IMemoryCache memoryCache, 
            IOptions<ClientRateLimitOptions> options = null, 
            IOptions<ClientRateLimitPolicies> policies = null)
        {
            _memoryCache = memoryCache;

            //save client rules defined in appsettings in cache on startup
            if(options != null && options.Value != null && policies != null && policies.Value != null && policies.Value.RuleSet != null)
            {
                foreach (var rule in policies.Value.RuleSet)
                {
                    Set($"{options.Value.PolicyPrefix}_{rule.Id}", new RateLimitPolicy { Id = rule.Id, Rules = rule.Rules });
                }
            }
        }

        public void Set(string id, RateLimitPolicy policy)
        {
            _memoryCache.Set(id, policy);
        }

        public bool Exists(string id)
        {
            RateLimitPolicy stored;
            return _memoryCache.TryGetValue(id, out stored);
        }

        public RateLimitPolicy Get(string id)
        {
            RateLimitPolicy stored;
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
