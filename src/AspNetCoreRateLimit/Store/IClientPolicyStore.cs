namespace AspNetCoreRateLimit
{
    public interface IClientPolicyStore
    {
        bool Exists(string id);
        RateLimitPolicy Get(string id);
        void Remove(string id);
        void Set(string id, RateLimitPolicy policy);
    }
}