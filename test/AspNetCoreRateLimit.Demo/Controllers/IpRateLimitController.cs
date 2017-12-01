using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AspNetCoreRateLimit.Demo.Controllers
{
    [Route("api/[controller]")]
    public class IpRateLimitController : Controller
    {
        private readonly RateLimitOptions _options;
        private readonly IPolicyStore<RateLimitPolicies> _ipPolicyStore;

        public IpRateLimitController(IOptions<RateLimitOptions> optionsAccessor, IPolicyStore<RateLimitPolicies> policyStore)
        {
            _options = optionsAccessor.Value;
            _ipPolicyStore = policyStore;
        }

        [HttpGet]
        public RateLimitPolicies Get()
        {
            return _ipPolicyStore.Get(_options.PolicyPrefix);
        }

        [HttpPost]
        public void Post()
        {
            var pol = _ipPolicyStore.Get(_options.PolicyPrefix) ?? new RateLimitPolicies();

            pol.RuleSet.Add(new RateLimitPolicy
            {
                Id = "8.8.4.4",
                Rules = new List<RateLimitRule>(new RateLimitRule[] {
                    new RateLimitRule {
                        Endpoint = "*:/api/testupdate",
                        Limit = 100,
                        Period = "1d" }
                })
            });

            _ipPolicyStore.Set(_options.PolicyPrefix, pol);
        }
    }
}
