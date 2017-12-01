using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace AspNetCoreRateLimit
{
    public class IpRateLimitMiddleware : RateLimitMiddleware
    {
        public IpRateLimitMiddleware(RequestDelegate next, 
            IOptions<IpRateLimitOptions> options,
            IRateLimitCounterStore counterStore,
            IIpPolicyStore policyStore,
            ILogger<IpRateLimitMiddleware> logger
            ) : base(next, options, counterStore, policyStore, logger)
        {
            //Processor = new IpRateLimitProcessor(options.Value, counterStore, policyStore, new RemoteIpParser());
        }

        public override void LogBlockedRequest(HttpContext httpContext, RequestIdentity identity, RateLimitCounter counter, RateLimitRule rule)
        {
            _logger.LogInformation($"Request {identity.HttpVerb}:{identity.Path} from IP {identity.ClientIp} has been blocked, quota {rule.Limit}/{rule.Period} exceeded by {counter.TotalRequests}. Blocked by rule {rule.Endpoint}, TraceIdentifier {httpContext.TraceIdentifier}.");
        }
    }
}
