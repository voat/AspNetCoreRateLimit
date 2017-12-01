namespace AspNetCoreRateLimit
{
    public interface IIpPolicyStore : IPolicyStore<RateLimitPolicies>
    {
        //bool Exists(string id);
        //RateLimitPolicies Get(string id);
        //void Remove(string id);
        //void Set(string id, RateLimitPolicies policy);
    }
}