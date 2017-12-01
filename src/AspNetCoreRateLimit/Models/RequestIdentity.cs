namespace AspNetCoreRateLimit
{
    /// <summary>
    /// Stores the client IP, ID, endpoint and verb
    /// </summary>
    public class RequestIdentity
    {
        public RequestIdentity(IdentityMask identityIntersection)
        {
            Identity = identityIntersection;
        }

        public IdentityMask Identity { get; set; }

        public string ClientIp { get; set; }

        public string ClientId { get; set; }

        /// <summary>
        /// Returns the identifier of this request identity object based upon the IdentityMask specified
        /// </summary>
        public virtual string UniqueId 
        {
            get
            {
                switch (Identity)
                {
                    case IdentityMask.Ip:
                        return ClientIp;
                    case IdentityMask.IdAndIp:
                        return $"{ClientId}|{ClientIp}";
                    case IdentityMask.Id:
                    default:
                        return ClientId;
                }
            }
        }

        public string Path { get; set; }

        public string HttpVerb { get; set; }
    }
}
