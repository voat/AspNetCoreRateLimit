using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AspNetCoreRateLimit
{
    public class RateLimitProcessor : RateLimitCore, IRateLimitProcessor
    {

        public RateLimitProcessor(
            RateLimitOptions options, 
            IRateLimitCounterStore counterStore,
            IPolicyStore<RateLimitPolicies> policyStore,
            IIpAddressParser ipAddressParser) : base(options, counterStore, policyStore, ipAddressParser)
        {
        }

        public virtual List<RateLimitRule> GetMatchingRules(RequestIdentity identity)
        {
            var limits = new List<RateLimitRule>();
            var policies = PolicyStore.Get($"{Options.PolicyPrefix}");

            var filteredPolicies = FilterRuleSet(policies, identity);
            //var policy = policies?.RuleSet.Where(x => 
            //        (x.Type == Options.Identity || (x.Type == IdentityMask.Id && Options.Identity == IdentityMask.IdAndIp)) 
            //        && x.Id == identity.ClientId
            //    ).FirstOrDefault();
            if (filteredPolicies != null && filteredPolicies.Any())
            {
                var rules = filteredPolicies.SelectMany(x => x.Rules).ToList();

                if (Options.EnableEndpointRateLimiting)
                {
                    // search for rules with endpoints like "*" and "*:/matching_path"
                    var pathLimits = rules.Where(l => $"*:{identity.Path}".ContainsIgnoreCase(l.Endpoint)).AsEnumerable();
                    limits.AddRange(pathLimits);

                    // search for rules with endpoints like "matching_verb:/matching_path"
                    var verbLimits = rules.Where(l => $"{identity.HttpVerb}:{identity.Path}".ContainsIgnoreCase(l.Endpoint)).AsEnumerable();
                    limits.AddRange(verbLimits);
                }
                else
                {
                    //ignore endpoint rules and search for global rules only
                    var genericLimits = rules.Where(l => l.Endpoint == "*").AsEnumerable();
                    limits.AddRange(genericLimits);
                }
            }

            // get the most restrictive limit for each period 
            limits = limits.GroupBy(l => l.Period).Select(l => l.OrderBy(x => x.Limit)).Select(l => l.First()).ToList();

            // search for matching general rules
            if (Options.GeneralRules != null)
            {
                var matchingGeneralLimits = new List<RateLimitRule>();
                if (Options.EnableEndpointRateLimiting)
                {
                    // search for rules with endpoints like "*" and "*:/matching_path" in general rules
                    var pathLimits = Options.GeneralRules.Where(l => $"*:{identity.Path}".ContainsIgnoreCase(l.Endpoint)).AsEnumerable();
                    matchingGeneralLimits.AddRange(pathLimits);

                    // search for rules with endpoints like "matching_verb:/matching_path" in general rules
                    var verbLimits = Options.GeneralRules.Where(l => $"{identity.HttpVerb}:{identity.Path}".ContainsIgnoreCase(l.Endpoint)).AsEnumerable();
                    matchingGeneralLimits.AddRange(verbLimits);
                }
                else
                {
                    //ignore endpoint rules and search for global rules in general rules
                    var genericLimits = Options.GeneralRules.Where(l => l.Endpoint == "*").AsEnumerable();
                    matchingGeneralLimits.AddRange(genericLimits);
                }

                // get the most restrictive general limit for each period 
                var generalLimits = matchingGeneralLimits.GroupBy(l => l.Period).Select(l => l.OrderBy(x => x.Limit)).Select(l => l.First()).ToList();

                foreach (var generalLimit in generalLimits)
                {
                    // add general rule if no specific rule is declared for the specified period
                    if (!limits.Exists(l => l.Period == generalLimit.Period))
                    {
                        limits.Add(generalLimit);
                    }
                }
            }

            foreach (var item in limits)
            {
                //parse period text into time spans
                item.PeriodTimespan = ConvertToTimeSpan(item.Period);
            }

            limits = limits.OrderBy(l => l.PeriodTimespan).ToList();
            if (Options.StackBlockedRequests)
            {
                limits.Reverse();
            }

            return limits;
        }

        public virtual bool IsWhitelisted(RequestIdentity requestIdentity)
        {
            if (Options.IpWhitelist != null && IpParser.ContainsIp(Options.IpWhitelist, requestIdentity.ClientIp))
            {
                return true;
            }

            if (Options.ClientWhitelist != null && Options.ClientWhitelist.Contains(requestIdentity.ClientId))
            {
                return true;
            }

            if (Options.EndpointWhitelist != null && Options.EndpointWhitelist.Any())
            {
                if (Options.EndpointWhitelist.Any(x => $"{requestIdentity.HttpVerb}:{requestIdentity.Path}".ToLowerInvariant().Contains(x.ToLowerInvariant())) ||
                    Options.EndpointWhitelist.Any(x => $"*:{requestIdentity.Path}".ToLowerInvariant().Contains(x.ToLowerInvariant())))
                    return true;
            }

            return false;
        }

        public virtual IEnumerable<RateLimitPolicy> FilterRuleSet(RateLimitPolicies policies, RequestIdentity identity)
        {
            switch (Options.Identity)
            {
                case IdentityMask.Id:
                    return policies?.RuleSet.Where(x => 
                    ((x.Type & IdentityMask.Id) > 0)
                    && x.Id == identity.ClientId);
                    break;
                case IdentityMask.Ip:
                    return policies?.RuleSet.Where(x =>
                    ((x.Type & IdentityMask.Ip) > 0)
                    && IpParser.ContainsIp(x.Id, identity.ClientIp));
                    break;
                case IdentityMask.IdAndIp:
                default:
                    return policies?.RuleSet.Where(x =>
                    (
                        ((x.Type & IdentityMask.Ip) > 0)
                        && IpParser.ContainsIp(x.Id, identity.ClientIp)
                    )
                    ||
                    (
                        ((x.Type & IdentityMask.Id) > 0)
                        && x.Id == identity.ClientId)
                    );
                    break;
            }

        }
    }
}
