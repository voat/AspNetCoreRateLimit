using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCoreRateLimit
{
    public interface IPolicyStore<T>
    {
        bool Exists(string id);
        T Get(string id);
        void Remove(string id);
        void Set(string id, T policy);
    }
}
