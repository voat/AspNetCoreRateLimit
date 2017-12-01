using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace AspNetCoreRateLimit
{
    public class ReversProxyIpParser : RemoteIpParser
    {
        private readonly string[] _realIpHeaders;

        /// <summary>
        /// A header name or a list of headers with a comma seperator
        /// </summary>
        /// <param name="realIpHeaders"></param>
        public ReversProxyIpParser(string realIpHeaders)
        {
            _realIpHeaders = realIpHeaders.Split(new [] { "," }, StringSplitOptions.RemoveEmptyEntries );
        }

        public override IPAddress GetClientIp(HttpContext context)
        {
            foreach (string headerName in _realIpHeaders)
            {
                if (context.Request.Headers.Keys.Contains(headerName, StringComparer.CurrentCultureIgnoreCase))
                {
                    return ParseIp(context.Request.Headers[headerName].Last());
                }
            }

            return base.GetClientIp(context);
        }
    }
}
