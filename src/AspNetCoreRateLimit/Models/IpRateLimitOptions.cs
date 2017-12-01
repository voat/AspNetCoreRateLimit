using System.Collections.Generic;

namespace AspNetCoreRateLimit
{
    public class IpRateLimitOptions : RateLimitOptions
    {
        public IpRateLimitOptions()
        {
            PolicyPrefix = "ippp";
        }
    }
}
