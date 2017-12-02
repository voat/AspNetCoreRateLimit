﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit
{
    public static class Extensions
    {
        public static bool ContainsIgnoreCase(this string source, string value, StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
        {
            return source != null && value != null && source.IndexOf(value, stringComparison) >= 0;
        }
        public static TimeSpan ConvertToTimeSpan(this string timeSpan)
        {
            var l = timeSpan.Length - 1;
            var value = timeSpan.Substring(0, l);
            var type = timeSpan.Substring(l, 1);

            switch (type)
            {
                case "d": return TimeSpan.FromDays(double.Parse(value));
                case "h": return TimeSpan.FromHours(double.Parse(value));
                case "m": return TimeSpan.FromMinutes(double.Parse(value));
                case "s": return TimeSpan.FromSeconds(double.Parse(value));
                default: throw new FormatException($"{timeSpan} can't be converted to TimeSpan, unknown type {type}");
            }
        }
    }
}
