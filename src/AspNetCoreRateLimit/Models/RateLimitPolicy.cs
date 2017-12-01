using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCoreRateLimit
{
    public class RateLimitPolicy
    {
        public string Id { get; set; }
        public IdentityMask Type { get; set; }
        public List<RateLimitRule> Rules { get; set; } = new List<RateLimitRule>();
    }
}
