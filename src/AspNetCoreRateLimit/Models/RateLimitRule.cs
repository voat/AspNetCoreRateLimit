using System;

namespace AspNetCoreRateLimit
{
    public class RateLimitRule
    {
        private string _period = "0s";
        /// <summary>
        /// HTTP verb and path 
        /// </summary>
        /// <example>
        /// get:/api/values
        /// *:/api/values
        /// *
        /// </example>
        public string Endpoint { get; set; }

        /// <summary>
        /// Rate limit period as in 1s, 1m, 1h
        /// </summary>
        public string Period {
            get => _period;
            set {
                PeriodTimespan = value.ConvertToTimeSpan();
                _period = value;
            }
        }

        public TimeSpan PeriodTimespan { get; private set; } = TimeSpan.Zero;

        /// <summary>
        /// Maximum number of requests that a client can make in a defined period
        /// </summary>
        public long Limit { get; set; }
    }
}
