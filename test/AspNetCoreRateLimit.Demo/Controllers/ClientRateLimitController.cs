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
    public class ClientRateLimitController : Controller
    {
        private readonly RateLimitOptions _options;
        private readonly IPolicyStore<RateLimitPolicies> _policyStore;

        public ClientRateLimitController(IOptions<RateLimitOptions> optionsAccessor, IPolicyStore<RateLimitPolicies> policyStore)
        {
            _options = optionsAccessor.Value;
            _policyStore = policyStore;
        }

        [HttpGet]
        public RateLimitPolicy Get()
        {
            return _policyStore.Get($"{_options.PolicyPrefix}")?.RuleSet.Where(x => x.Id == "cl-key-1").FirstOrDefault();
        }

        [HttpPost]
        public void Post()
        {
            //var id = $"{_options.PolicyPrefix}_cl-key-1";
            var policies = _policyStore.Get($"{_options.PolicyPrefix}");
            var anonPolicy = policies?.RuleSet.Where(x => x.Id == "cl-key-1").FirstOrDefault();
            anonPolicy.Rules.Add(new RateLimitRule
            {
                Endpoint = "*/api/testpolicyupdate",
                Period = "1h",
                Limit = 100
            });
            _policyStore.Set($"{_options.PolicyPrefix}", policies);
        }
    }
}
