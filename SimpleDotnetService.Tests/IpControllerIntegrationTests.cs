using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;

namespace SimpleDotnetService.Tests
{
    public class IpControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> factory;
        private readonly HttpClient client;

        public IpControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            this.factory = factory;
            this.client = factory.CreateClient();
        }

        [Fact(Skip = "Requires external API access to api.ipify.org")]
        public async Task GetOutboundIp_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await client.GetAsync("/api/ip/outbound");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("outboundip", content);
        }

        [Fact(Skip = "Requires external API access to api.ipify.org")]
        public async Task GetOutboundIp_ReturnsValidJsonWithIpAddress()
        {
            // Act
            var response = await client.GetAsync("/api/ip/outbound");

            // Assert
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(content);
            
            Assert.True(jsonDocument.RootElement.TryGetProperty("outboundip", out var ipElement));
            var ipAddress = ipElement.GetString();
            Assert.NotNull(ipAddress);
            Assert.NotEmpty(ipAddress);
        }

        [Fact]
        public async Task GetInboundIp_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await client.GetAsync("/api/ip/inbound");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("inboundip", content);
        }

        [Fact]
        public async Task GetInboundIp_ReturnsValidJsonWithIpAddress()
        {
            // Act
            var response = await client.GetAsync("/api/ip/inbound");

            // Assert
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(content);
            
            Assert.True(jsonDocument.RootElement.TryGetProperty("inboundip", out var ipElement));
            var ipAddress = ipElement.GetString();
            Assert.NotNull(ipAddress);
            Assert.NotEmpty(ipAddress);
        }

        [Fact]
        public async Task GetHeaders_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await client.GetAsync("/api/ip/headers");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetHeaders_ReturnsValidJsonDictionary()
        {
            // Act
            var response = await client.GetAsync("/api/ip/headers");

            // Assert
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(content);
            
            Assert.NotNull(headers);
        }

        [Fact]
        public async Task GetHeaders_IncludesRequestHeaders()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/ip/headers");
            request.Headers.Add("X-Custom-Test-Header", "TestValue");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(content, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            Assert.NotNull(headers);
            Assert.Contains("X-Custom-Test-Header", headers.Keys);
            Assert.Equal("TestValue", headers["X-Custom-Test-Header"]);
        }

        [Fact]
        public async Task GetHeaders_ContainsUserAgentHeader()
        {
            // Act
            var response = await client.GetAsync("/api/ip/headers");

            // Assert
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            Assert.NotNull(headers);
            // The default HttpClient should have a User-Agent or Host header
            Assert.True(headers.Count > 0, "Should have at least one header");
        }
    }
}
