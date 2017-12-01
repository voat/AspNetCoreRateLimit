namespace AspNetCoreRateLimit
{
    public class ClientRateLimitOptions : RateLimitOptions
    {
        public ClientRateLimitOptions()
        {
            PolicyPrefix = "crlp";
        }
    }
}
