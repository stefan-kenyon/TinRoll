using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TinRoll.Integration
{
    public class QuestionIntegrationTests
        :  IClassFixture<WebApplicationFactory<Server.Startup>>
    {
        private readonly WebApplicationFactory<Server.Startup> _factory;
        public QuestionIntegrationTests(WebApplicationFactory<Server.Startup> factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("/api/Question")]
        public async Task Get_EndpointsTestSuccessResponse(string url)
        {
            //Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json", response.Content.Headers.ContentType.ToString());
        }
    }
}
