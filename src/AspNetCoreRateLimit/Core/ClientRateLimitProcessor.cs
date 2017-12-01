using System;
using System.Collections.Generic;
using System.Linq;

namespace AspNetCoreRateLimit
{
    public class ClientRateLimitProcessor : RateLimitProcessor
    {
        //private readonly IPolicyStore<RateLimitPolicies> _policyStore;
        
        public ClientRateLimitProcessor(ClientRateLimitOptions options,
           IRateLimitCounterStore counterStore,
           IPolicyStore<RateLimitPolicies> policyStore,
           IIpAddressParser ipAddressParser) : base(options, counterStore, policyStore, ipAddressParser)
        {
            //_policyStore = policyStore;
        }

        //public override bool IsWhitelisted(RequestIdentity requestIdentity)
        //{
        //    if (Options.ClientWhitelist != null && Options.ClientWhitelist.Contains(requestIdentity.ClientId))
        //    {
        //        return true;
        //    }

        //    if (Options.EndpointWhitelist != null && Options.EndpointWhitelist.Any())
        //    {
        //        if (Options.EndpointWhitelist.Any(x => $"{requestIdentity.HttpVerb}:{requestIdentity.Path}".ContainsIgnoreCase(x)) ||
        //            Options.EndpointWhitelist.Any(x => $"*:{requestIdentity.Path}".ContainsIgnoreCase(x)))
        //            return true;
        //    }

        //    return false;
        //}

       
    }
}
