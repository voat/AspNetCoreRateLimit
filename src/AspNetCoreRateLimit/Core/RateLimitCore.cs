﻿using System;
using System.Globalization;

namespace AspNetCoreRateLimit
{
    public class RateLimitCore
    {
        private readonly RateLimitOptions _options;
        private readonly IRateLimitCounterStore _counterStore;
        private readonly IIpAddressParser _ipParser;
        private static readonly object _processLocker = new object();
        private readonly IPolicyStore<RateLimitPolicies> _policyStore;
        public RateLimitOptions Options => _options;

        public IIpAddressParser IpParser => _ipParser;

        public IPolicyStore<RateLimitPolicies> PolicyStore => _policyStore;

        public RateLimitCore(
           RateLimitOptions options,
           IRateLimitCounterStore counterStore,
           IPolicyStore<RateLimitPolicies> policyStore,
           IIpAddressParser ipParser)
        {
            _options = options;
            _counterStore = counterStore;
            _policyStore = policyStore;
            _ipParser = ipParser;

        }

        public string ComputeCounterKey(RequestIdentity requestIdentity, RateLimitRule rule)
        {
            var key = $"{_options.RateLimitCounterPrefix}_{requestIdentity.UniqueId}_{rule.Period}";
            //var key = _ipRateLimiting ?
            //   $"{_options.RateLimitCounterPrefix}_{requestIdentity.ClientIp}_{rule.Period}" :
            //   $"{_options.RateLimitCounterPrefix}_{requestIdentity.ClientId}_{rule.Period}";

            if (_options.EnableEndpointRateLimiting)
            {
                key += $"_{requestIdentity.HttpVerb}_{requestIdentity.Path}";

                // TODO: consider using the rule endpoint as key, this will allow to rate limit /api/values/1 and api/values/2 under same counter
                //key += $"_{rule.Endpoint}";
            }

            var idBytes = System.Text.Encoding.UTF8.GetBytes(key);

            byte[] hashBytes;

            using (var algorithm = System.Security.Cryptography.SHA1.Create())
            {
                hashBytes = algorithm.ComputeHash(idBytes);
            }

            return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        }

        public RateLimitCounter ProcessRequest(RequestIdentity requestIdentity, RateLimitRule rule)
        {
            var counter = new RateLimitCounter
            {
                Timestamp = DateTime.UtcNow,
                TotalRequests = 1
            };

            var counterId = ComputeCounterKey(requestIdentity, rule);

            // serial reads and writes
            lock (_processLocker)
            {
                var entry = _counterStore.Get(counterId);
                if (entry.HasValue)
                {
                    // entry has not expired
                    if (entry.Value.Timestamp + rule.PeriodTimespan >= DateTime.UtcNow)
                    {
                        // increment request count
                        var totalRequests = entry.Value.TotalRequests + 1;

                        // deep copy
                        counter = new RateLimitCounter
                        {
                            Timestamp = entry.Value.Timestamp,
                            TotalRequests = totalRequests
                        };
                    }
                }

                // stores: id (string) - timestamp (datetime) - total_requests (long)
                _counterStore.Set(counterId, counter, rule.PeriodTimespan);
            }

            return counter;
        }

        public RateLimitHeaders GetRateLimitHeaders(RequestIdentity requestIdentity, RateLimitRule rule)
        {
            var headers = new RateLimitHeaders();
            var counterId = ComputeCounterKey(requestIdentity, rule);
            var entry = _counterStore.Get(counterId);
            if (entry.HasValue)
            {
                headers.Reset = (entry.Value.Timestamp + rule.PeriodTimespan).ToUniversalTime().ToString("o", DateTimeFormatInfo.InvariantInfo);
                headers.Limit = rule.Period;
                headers.Remaining = (rule.Limit - entry.Value.TotalRequests).ToString();
            }
            else
            {
                headers.Reset = (DateTime.UtcNow + rule.PeriodTimespan).ToUniversalTime().ToString("o", DateTimeFormatInfo.InvariantInfo);
                headers.Limit = rule.Period;
                headers.Remaining = rule.Limit.ToString();
            }

            return headers;
        }

        public string RetryAfterFrom(DateTime timestamp, RateLimitRule rule)
        {
            var secondsPast = Convert.ToInt32((DateTime.UtcNow - timestamp).TotalSeconds);
            var retryAfter = Convert.ToInt32(rule.PeriodTimespan.TotalSeconds);
            retryAfter = retryAfter > 1 ? retryAfter - secondsPast : 1;
            return retryAfter.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

    }
}
