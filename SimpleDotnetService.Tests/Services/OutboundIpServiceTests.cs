using Microsoft.Extensions.Logging;
using Moq;
using SimpleDotnetService.Proxies;
using SimpleDotnetService.Services;
using SimpleDotnetService.Services.Ip;
using Xunit;

namespace SimpleDotnetService.Tests.Services
{
    public class OutboundIpServiceTests
    {
        private readonly Mock<ILogger<OutboundIpService>> mockLogger;
        private readonly Mock<IIpifyProxy> mockIpifyProxy;
        private readonly IIpAddressService service;

        public OutboundIpServiceTests()
        {
            mockLogger = new Mock<ILogger<OutboundIpService>>();
            mockIpifyProxy = new Mock<IIpifyProxy>();
            service = new OutboundIpService(mockLogger.Object, mockIpifyProxy.Object);
        }

        [Fact]
        public async Task GetOutboundIpAsync_ReturnsIpAddress_WhenProxySucceeds()
        {
            var expectedIp = "203.0.113.42";
            mockIpifyProxy.Setup(p => p.GetIpAsync()).ReturnsAsync(expectedIp);

            var result = await service.GetOutboundIpAsync();

            Assert.Equal(expectedIp, result);
            mockIpifyProxy.Verify(p => p.GetIpAsync(), Times.Once);
        }

        [Fact]
        public async Task GetOutboundIpAsync_ThrowsException_WhenProxyFails()
        {
            var expectedException = new HttpRequestException("Network error");
            mockIpifyProxy.Setup(p => p.GetIpAsync()).ThrowsAsync(expectedException);

            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => service.GetOutboundIpAsync());
            
            Assert.Equal(expectedException, exception);
            mockIpifyProxy.Verify(p => p.GetIpAsync(), Times.Once);
        }

        [Fact]
        public async Task GetOutboundIpAsync_LogsInformation_OnSuccess()
        {
            var expectedIp = "192.168.1.1";
            mockIpifyProxy.Setup(p => p.GetIpAsync()).ReturnsAsync(expectedIp);

            await service.GetOutboundIpAsync();

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Getting outbound IP address")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrieved outbound IP")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetOutboundIpAsync_LogsError_OnFailure()
        {
            var expectedException = new HttpRequestException("Network error");
            mockIpifyProxy.Setup(p => p.GetIpAsync()).ThrowsAsync(expectedException);

            await Assert.ThrowsAsync<HttpRequestException>(() => service.GetOutboundIpAsync());

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to retrieve outbound IP address")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData("10.0.0.1")]
        [InlineData("172.16.0.1")]
        [InlineData("192.168.1.100")]
        [InlineData("203.0.113.42")]
        public async Task GetOutboundIpAsync_ReturnsCorrectIp_ForVariousIpAddresses(string expectedIp)
        {
            mockIpifyProxy.Setup(p => p.GetIpAsync()).ReturnsAsync(expectedIp);

            var result = await service.GetOutboundIpAsync();

            Assert.Equal(expectedIp, result);
        }

        [Fact]
        public async Task GetOutboundIpAsync_CallsProxyOnlyOnce()
        {
            var expectedIp = "203.0.113.42";
            mockIpifyProxy.Setup(p => p.GetIpAsync()).ReturnsAsync(expectedIp);

            await service.GetOutboundIpAsync();

            mockIpifyProxy.Verify(p => p.GetIpAsync(), Times.Once);
            mockIpifyProxy.VerifyNoOtherCalls();
        }
    }
}
