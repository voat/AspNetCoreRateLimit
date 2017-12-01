using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCoreRateLimit.Tests
{
    public class EndPointTests
    {
        [Fact]
        public async Task WildCardEndpoint()
        {
            var endpointList = new[] { "*" };

            Assert.True(EndPoint.HasMatch(endpointList, "get", "/some/path"));
            Assert.True(EndPoint.HasMatch(endpointList, "post", "/some/path"));

        }
        //[Fact]
        //public async Task SimilarStart()
        //{
        //    //Need to fix this... someday
        //    var endpointList = new[] { "*:/api/" };
        //    Assert.True(EndPoint.HasMatch(endpointList, "get", "/api"));
        //    Assert.False(EndPoint.HasMatch(endpointList, "get", "/api2"));
        //}
        [Fact]
        public async Task WildCardMethod()
        {
            var endpointList = new[] { "*:/some/path" };

            Assert.True(EndPoint.HasMatch(endpointList, "get", "/some/path"));
            Assert.True(EndPoint.HasMatch(endpointList, "update", "/some/path"));
            Assert.True(EndPoint.HasMatch(endpointList, "get", "/some/Path"));

            Assert.True(EndPoint.HasMatch(endpointList, "poSt", "/some/Path/"));
            Assert.False(EndPoint.HasMatch(endpointList, "post", "/some/curve/ball"));

        }
        [Fact]
        public async Task WildCardPath()
        {
            var endpointList = new[] { "post:*", "get:*" };

            Assert.True(EndPoint.HasMatch(endpointList, "post", "/some/path"));
            Assert.True(EndPoint.HasMatch(endpointList, "gEt", "/some/path"));
            Assert.False(EndPoint.HasMatch(endpointList, "delete", "/some/path"));
        }
        [Fact]
        public async Task EmptyEndpointList()
        {
            var endpointList = new string[0];

            Assert.False(EndPoint.HasMatch(endpointList, "post", "/some/path"));
            Assert.False(EndPoint.HasMatch(endpointList, "get", "/some/path"));
            Assert.False(EndPoint.HasMatch(endpointList, "delEte", "/some/path"));
        }
        [Fact]
        public async Task NullEndpointList()
        {
            var endpointList = (string[])null;

            Assert.False(EndPoint.HasMatch(endpointList, "post", "/some/path"));
            Assert.False(EndPoint.HasMatch(endpointList, "get", "/some/path"));
            Assert.False(EndPoint.HasMatch(endpointList, "delete", "/some/path"));
        }
    }
}
