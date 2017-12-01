﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        protected readonly ILogger _logger;
        protected readonly RateLimitOptions _options;
        private IRateLimitProcessor _processor;
        private readonly IIpAddressParser _ipParser;

        //private readonly RateLimitO
        public RateLimitMiddleware(RequestDelegate next,
            IOptions<RateLimitOptions> options,
            IRateLimitCounterStore counterStore,
            IPolicyStore<RateLimitPolicies> policyStore, 
            ILogger<RateLimitMiddleware> logger,
            IIpAddressParser ipParser = null)
        {
            _next = next;
            _logger = logger;
            _options = options.Value;
            _ipParser = ipParser == null ? new ReversProxyIpParser(_options.RealIpHeader) : ipParser;
            _processor = new RateLimitProcessor(_options, counterStore, policyStore, _ipParser);
        }
        protected IRateLimitProcessor Processor { get => _processor; set => _processor = value; }
        protected IIpAddressParser IpParser { get => _ipParser; }

        public virtual RequestIdentity SetIdentity(HttpContext httpContext)
        {
            var clientId = "anon";
            if (httpContext.Request.Headers.Keys.Contains(_options.ClientIdHeader))
            {
                clientId = httpContext.Request.Headers[_options.ClientIdHeader].First();
            }

            var clientIp = string.Empty;
            try
            {
                var ip = IpParser.GetClientIp(httpContext);
                if (ip != null)
                {
                    //throw new Exception("IpRateLimitMiddleware can't parse caller IP");
                    clientIp = ip.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("RateLimitMiddleware can't parse caller IP", ex);
            }

            return new RequestIdentity(_options.Identity)
            {
                ClientId = clientId,
                ClientIp = clientIp,
                Path = httpContext.Request.Path.ToString().ToLowerInvariant(),
                HttpVerb = httpContext.Request.Method.ToLowerInvariant()
            };
        }

        public async Task Invoke(HttpContext httpContext)
        {
            // check if rate limiting is enabled
            if (_options != null)
            {
                if (!_options.MonitoredPaths.Any() || EndPoint.HasMatch(_options.MonitoredPaths, httpContext.Request))
                {
                    // compute identity from request
                    var identity = SetIdentity(httpContext);

                    // check white list
                    if (_processor.IsWhitelisted(identity))
                    {
                        await _next.Invoke(httpContext);
                        return;
                    }

                    var rules = _processor.GetMatchingRules(identity);

                    foreach (var rule in rules)
                    {
                        if (rule.Limit > 0)
                        {
                            // increment counter
                            var counter = _processor.ProcessRequest(identity, rule);

                            // check if key expired
                            if (counter.Timestamp + rule.PeriodTimespan.Value < DateTime.UtcNow)
                            {
                                continue;
                            }

                            // check if limit is reached
                            if (counter.TotalRequests > rule.Limit)
                            {
                                //compute retry after value
                                var retryAfter = _processor.RetryAfterFrom(counter.Timestamp, rule);

                                // log blocked request
                                LogBlockedRequest(httpContext, identity, counter, rule);

                                // break execution
                                await ReturnQuotaExceededResponse(httpContext, rule, retryAfter);
                                return;
                            }
                        }
                    }

                    //set X-Rate-Limit headers for the longest period
                    if (rules.Any() && !_options.DisableRateLimitHeaders)
                    {
                        var rule = rules.OrderByDescending(x => x.PeriodTimespan.Value).First();
                        var headers = _processor.GetRateLimitHeaders(identity, rule);
                        headers.Context = httpContext;

                        httpContext.Response.OnStarting(SetRateLimitHeaders, state: headers);
                    }
                }
            }
            await _next.Invoke(httpContext);
        }
        public virtual void LogBlockedRequest(HttpContext httpContext, RequestIdentity identity, RateLimitCounter counter, RateLimitRule rule)
        {
            _logger.LogInformation($"Request {identity.HttpVerb}:{identity.Path} from ClientId {identity.ClientId} has been blocked, quota {rule.Limit}/{rule.Period} exceeded by {counter.TotalRequests}. Blocked by rule {rule.Endpoint}, TraceIdentifier {httpContext.TraceIdentifier}.");
        }

        public virtual Task ReturnQuotaExceededResponse(HttpContext httpContext, RateLimitRule rule, string retryAfter)
        {
            var message = string.IsNullOrEmpty(_options.QuotaExceededMessage) ? $"API calls quota exceeded! maximum admitted {rule.Limit} per {rule.Period}." : _options.QuotaExceededMessage;

            if (!_options.DisableRateLimitHeaders)
            {
                httpContext.Response.Headers["Retry-After"] = retryAfter;
            }

            httpContext.Response.StatusCode = _options.HttpStatusCode;
            return httpContext.Response.WriteAsync(message);
        }

        private Task SetRateLimitHeaders(object rateLimitHeaders)
        {
            var headers = (RateLimitHeaders)rateLimitHeaders;

            headers.Context.Response.Headers["X-Rate-Limit-Limit"] = headers.Limit;
            headers.Context.Response.Headers["X-Rate-Limit-Remaining"] = headers.Remaining;
            headers.Context.Response.Headers["X-Rate-Limit-Reset"] = headers.Reset;

            return Task.CompletedTask;
        }
    }
}
