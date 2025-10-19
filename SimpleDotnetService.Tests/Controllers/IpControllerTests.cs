using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SimpleDotnetService.Controllers;
using SimpleDotnetService.Services;
using Xunit;

namespace SimpleDotnetService.Tests.Controllers
{
    public class IpControllerTests
    {
        private readonly Mock<IIpAddressService> mockIpAddressService;
        private readonly Mock<ILogger<IpController>> mockLogger;
        private readonly IpController controller;

        public IpControllerTests()
        {
            mockIpAddressService = new Mock<IIpAddressService>();
            mockLogger = new Mock<ILogger<IpController>>();
            controller = new IpController(mockIpAddressService.Object, mockLogger.Object);
        }

        [Fact]
        public async Task GetOutboundIp_ReturnsOkResult_WithIpAddress()
        {
            var expectedIp = "203.0.113.42";
            mockIpAddressService.Setup(s => s.GetOutboundIpAsync()).ReturnsAsync(expectedIp);

            var result = await controller.GetOutboundIp();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            
            Assert.NotNull(value);
            var outboundIpProperty = value.GetType().GetProperty("outboundip");
            Assert.NotNull(outboundIpProperty);
            var actualIp = outboundIpProperty.GetValue(value)?.ToString();
            Assert.Equal(expectedIp, actualIp);
        }

        [Fact]
        public async Task GetOutboundIp_ReturnsInternalServerError_WhenServiceThrowsException()
        {
            var expectedException = new Exception("Service error");
            mockIpAddressService.Setup(s => s.GetOutboundIpAsync()).ThrowsAsync(expectedException);

            var result = await controller.GetOutboundIp();

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            
            var value = statusCodeResult.Value;
            Assert.NotNull(value);
            var errorProperty = value.GetType().GetProperty("error");
            Assert.NotNull(errorProperty);
            var errorMessage = errorProperty.GetValue(value)?.ToString();
            Assert.Equal("An error occurred while retrieving the outbound IP address", errorMessage);
        }

        [Fact]
        public async Task GetOutboundIp_LogsInformation_OnSuccessfulRequest()
        {
            var expectedIp = "192.168.1.1";
            mockIpAddressService.Setup(s => s.GetOutboundIpAsync()).ReturnsAsync(expectedIp);

            await controller.GetOutboundIp();

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Received request to get outbound IP address")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetOutboundIp_LogsError_OnException()
        {
            var expectedException = new Exception("Service error");
            mockIpAddressService.Setup(s => s.GetOutboundIpAsync()).ThrowsAsync(expectedException);

            await controller.GetOutboundIp();

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while getting outbound IP address")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData("10.0.0.1")]
        [InlineData("172.16.0.1")]
        [InlineData("192.168.1.100")]
        [InlineData("203.0.113.42")]
        [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334")]
        public async Task GetOutboundIp_ReturnsCorrectIp_ForVariousIpAddresses(string expectedIp)
        {
            mockIpAddressService.Setup(s => s.GetOutboundIpAsync()).ReturnsAsync(expectedIp);

            var result = await controller.GetOutboundIp();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            Assert.NotNull(value);
            var outboundIpProperty = value.GetType().GetProperty("outboundip");
            Assert.NotNull(outboundIpProperty);
            var actualIp = outboundIpProperty.GetValue(value)?.ToString();
            Assert.Equal(expectedIp, actualIp);
        }

        [Fact]
        public async Task GetOutboundIp_CallsServiceOnlyOnce()
        {
            var expectedIp = "203.0.113.42";
            mockIpAddressService.Setup(s => s.GetOutboundIpAsync()).ReturnsAsync(expectedIp);

            await controller.GetOutboundIp();

            mockIpAddressService.Verify(s => s.GetOutboundIpAsync(), Times.Once);
            mockIpAddressService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetOutboundIp_HandlesHttpRequestException()
        {
            var expectedException = new HttpRequestException("Network error");
            mockIpAddressService.Setup(s => s.GetOutboundIpAsync()).ThrowsAsync(expectedException);

            var result = await controller.GetOutboundIp();

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetOutboundIp_HandlesInvalidOperationException()
        {
            var expectedException = new InvalidOperationException("Invalid operation");
            mockIpAddressService.Setup(s => s.GetOutboundIpAsync()).ThrowsAsync(expectedException);

            var result = await controller.GetOutboundIp();

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }
    }
}
