using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCoreRateLimit
{
    public class IpRateLimitPolicies : RateLimitPolicies
    {
    }
    public class ClientRateLimitPolicies : RateLimitPolicies
    {
    }
    public class RateLimitPolicies
    {
        public List<RateLimitPolicy> RuleSet { get; set; } = new List<RateLimitPolicy>();
    }
}
