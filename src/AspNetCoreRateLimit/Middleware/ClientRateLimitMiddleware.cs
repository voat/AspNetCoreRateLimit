using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public class ClientRateLimitMiddleware : RateLimitMiddleware
    {
        public ClientRateLimitMiddleware(RequestDelegate next,
            IOptions<ClientRateLimitOptions> options,
            IRateLimitCounterStore counterStore,
            IPolicyStore<RateLimitPolicies> policyStore,
            ILogger<ClientRateLimitMiddleware> logger
            ) : base(next, options, counterStore, policyStore, logger)
        {
            //Processor = new ClientRateLimitProcessor(options.Value, counterStore, policyStore, new RemoteIpParser());
        }
    }
}
