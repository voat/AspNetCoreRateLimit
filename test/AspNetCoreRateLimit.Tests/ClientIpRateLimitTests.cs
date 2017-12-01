using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCoreRateLimit.Tests
{
    public class ClientIpRateLimitTests : IClassFixture<RateLimitFixture<Demo.Startup>>
    {
        private const string apiPath = "/aPi/ClienTs";
        private const string apiRateLimitPath = "/api/ClientRateLimit";
        private const string ip = "::1";

        public ClientIpRateLimitTests(RateLimitFixture<Demo.Startup> fixture)
        {
            Client = fixture.Client;
        }

        public HttpClient Client { get; }

        [Theory]
        [InlineData("GET")]
        //[InlineData("PUT")]
        public async Task SpecificClientRule(string verb)
        {
            // Arrange
            var clientId = "cl-ip-key-1";
            var ip = "1.1.1.1";
            var expectedSuccessCount = 2;
            var requestCount = 10;

            await TestRequests(verb, clientId, ip, requestCount, expectedSuccessCount);
            
            //Different IP
            ip = "1.1.1.2";
            await TestRequests(verb, clientId, ip, requestCount, expectedSuccessCount);
            
            //Ensure client with previous IP is still blocked
            ip = "1.1.1.1";
            await TestRequests(verb, clientId, ip, requestCount, 0);

            //Change client
            ip = "1.1.1.1";
            clientId = "cl-ip-key-2";
            await TestRequests(verb, clientId, ip, requestCount, expectedSuccessCount);
        }

        private async Task TestRequests(string verb, string clientId, string ip, int requestCount, int expectedSuccessCount)
        {
            // Act    
            for (int i = 0; i < requestCount; i++)
            {
                var request = new HttpRequestMessage(new HttpMethod(verb), apiPath);
                request.Headers.Add("X-ClientId", clientId);
                request.Headers.Add("X-Real-IP", ip);

                var response = await Client.SendAsync(request);
                int responseStatusCode = (int)response.StatusCode;
                if (i < expectedSuccessCount)
                {
                    // Assert
                    Assert.NotEqual(429, responseStatusCode);
                }
                else
                {
                    // Assert
                    Assert.Equal(429, responseStatusCode);
                }
            }
        }
    }
}
