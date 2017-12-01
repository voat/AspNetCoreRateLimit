using System;
using System.Collections.Generic;

namespace AspNetCoreRateLimit
{
    public interface IRateLimitProcessor
    {
        List<RateLimitRule> GetMatchingRules(RequestIdentity identity);
        RateLimitHeaders GetRateLimitHeaders(RequestIdentity requestIdentity, RateLimitRule rule);
        bool IsWhitelisted(RequestIdentity requestIdentity);
        RateLimitCounter ProcessRequest(RequestIdentity requestIdentity, RateLimitRule rule);
        string RetryAfterFrom(DateTime timestamp, RateLimitRule rule);
    }
}