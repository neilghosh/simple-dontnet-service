using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using SimpleDotnetService.Proxies;
using Xunit;

namespace SimpleDotnetService.Tests.Proxies
{
    public class IpifyProxyTests
    {
        private readonly Mock<ILogger<IpifyProxy>> mockLogger;
        private readonly Mock<HttpMessageHandler> mockHttpMessageHandler;
        private readonly HttpClient httpClient;

        public IpifyProxyTests()
        {
            mockLogger = new Mock<ILogger<IpifyProxy>>();
            mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            httpClient = new HttpClient(mockHttpMessageHandler.Object);
        }

        [Fact]
        public async Task GetIpAsync_ReturnsIpAddress_WhenApiCallSucceeds()
        {
            var expectedIp = "203.0.113.42";
            var responseContent = JsonSerializer.Serialize(new { ip = expectedIp });
            
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });

            var proxy = new IpifyProxy(httpClient, mockLogger.Object);

            var result = await proxy.GetIpAsync();

            Assert.Equal(expectedIp, result);
        }

        [Fact]
        public async Task GetIpAsync_ReturnsUnknown_WhenIpIsNull()
        {
            var responseContent = JsonSerializer.Serialize(new { ip = (string?)null });
            
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });

            var proxy = new IpifyProxy(httpClient, mockLogger.Object);

            var result = await proxy.GetIpAsync();

            Assert.Equal("Unknown", result);
        }

        [Fact]
        public async Task GetIpAsync_ThrowsException_WhenApiCallFails()
        {
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var proxy = new IpifyProxy(httpClient, mockLogger.Object);

            await Assert.ThrowsAsync<HttpRequestException>(() => proxy.GetIpAsync());
        }

        [Fact]
        public async Task GetIpAsync_ThrowsException_WhenResponseIsInvalid()
        {
            var responseContent = "invalid json";
            
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });

            var proxy = new IpifyProxy(httpClient, mockLogger.Object);

            await Assert.ThrowsAsync<JsonException>(() => proxy.GetIpAsync());
        }

        [Fact]
        public async Task GetIpAsync_LogsInformation_OnSuccessfulCall()
        {
            var expectedIp = "192.168.1.1";
            var responseContent = JsonSerializer.Serialize(new { ip = expectedIp });
            
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });

            var proxy = new IpifyProxy(httpClient, mockLogger.Object);

            await proxy.GetIpAsync();

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Calling ipify API")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully retrieved IP")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetIpAsync_LogsError_OnFailure()
        {
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var proxy = new IpifyProxy(httpClient, mockLogger.Object);

            await Assert.ThrowsAsync<HttpRequestException>(() => proxy.GetIpAsync());

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to call ipify API")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
