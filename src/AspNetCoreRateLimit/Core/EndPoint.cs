using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AspNetCoreRateLimit
{
    public class EndPoint
    {
        public string Method { get; set; }
        public string Path { get; set; }

        public bool IsMatch(HttpRequest request)
        {
            return IsMatch(request.Method, request.Path);
        }
        public bool IsMatch(string method, string path)
        {
            return
                (Method == "*" || Method != "*" && Method.Equals(method, StringComparison.OrdinalIgnoreCase))
                &&
                (Path == "*" || Path != "*" && path.ToLowerInvariant().Trim('/').StartsWith(Path.Trim('/').ToLowerInvariant()));
        }

        public static EndPoint Parse(string endpointExpression)
        {
            if (endpointExpression == "*")
            {
                return new EndPoint() { Method = "*", Path = "*" };
            }
            else
            {
                var parts = endpointExpression.Split(':');
                return new EndPoint() { Method = parts[0], Path = parts[1] };
            }
        }
        public static bool HasMatch(IEnumerable<string> endpointExpressions, HttpRequest request)
        {
            return HasMatch(endpointExpressions, request.Method, request.Path);
        }
        public static bool HasMatch(IEnumerable<string> endpointExpressions, string method, string path)
        {
            if (endpointExpressions != null && endpointExpressions.Any())
            {
                return endpointExpressions.Any(x => Parse(x).IsMatch(method, path));
            }
            return false;
        }
    }
}
