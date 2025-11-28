using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace SimpleDotnetService.Tests.Integration
{
    public class ApplicationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> factory;

        public ApplicationIntegrationTests(WebApplicationFactory<Program> factory)
        {
            this.factory = factory;
        }

        [Fact]
        public async Task Health_ReturnsSuccessStatusCode()
        {
            var client = factory.CreateClient();

            var response = await client.GetAsync("/health");

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Health_ReturnsHealthyStatus()
        {
            var client = factory.CreateClient();

            var response = await client.GetAsync("/health");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal("Healthy", content);
        }
    }
}
