using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SimpleDotnetService.Controllers;
using SimpleDotnetService.Services;
using System.Net;

namespace SimpleDotnetService.Tests
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
        public async Task GetOutboundIp_ReturnsOkResult_WithOutboundIpAddress()
        {
            // Arrange
            var expectedIp = "203.0.113.42";
            mockIpAddressService.Setup(s => s.GetOutboundIpAsync())
                .ReturnsAsync(expectedIp);

            // Act
            var result = await controller.GetOutboundIp();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = okResult.Value;
            Assert.NotNull(returnValue);
            
            var ipProperty = returnValue.GetType().GetProperty("outboundip");
            Assert.NotNull(ipProperty);
            Assert.Equal(expectedIp, ipProperty.GetValue(returnValue));
        }

        [Fact]
        public async Task GetOutboundIp_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            mockIpAddressService.Setup(s => s.GetOutboundIpAsync())
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await controller.GetOutboundIp();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public void GetInboundIp_ReturnsOkResult_WithRemoteIpAddress()
        {
            // Arrange
            var remoteIpAddress = IPAddress.Parse("192.168.1.1");
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = remoteIpAddress;
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = controller.GetInboundIp();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = okResult.Value;
            Assert.NotNull(returnValue);
            
            var ipProperty = returnValue.GetType().GetProperty("inboundip");
            Assert.NotNull(ipProperty);
            Assert.Equal(remoteIpAddress.ToString(), ipProperty.GetValue(returnValue));
        }

        [Fact]
        public void GetInboundIp_ReturnsUnknown_WhenRemoteIpIsNull()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = null;
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = controller.GetInboundIp();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = okResult.Value;
            Assert.NotNull(returnValue);
            
            var ipProperty = returnValue.GetType().GetProperty("inboundip");
            Assert.NotNull(ipProperty);
            Assert.Equal("Unknown", ipProperty.GetValue(returnValue));
        }

        [Fact]
        public void GetHeaders_ReturnsOkResult_WithRequestHeaders()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["User-Agent"] = "TestAgent";
            httpContext.Request.Headers["Accept"] = "application/json";
            httpContext.Request.Headers["X-Custom-Header"] = "CustomValue";
            
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = controller.GetHeaders();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var headers = Assert.IsAssignableFrom<IDictionary<string, string>>(okResult.Value);
            
            Assert.Contains("User-Agent", headers.Keys);
            Assert.Contains("Accept", headers.Keys);
            Assert.Contains("X-Custom-Header", headers.Keys);
            Assert.Equal("TestAgent", headers["User-Agent"]);
            Assert.Equal("application/json", headers["Accept"]);
            Assert.Equal("CustomValue", headers["X-Custom-Header"]);
        }

        [Fact]
        public void GetHeaders_ReturnsEmptyDictionary_WhenNoHeaders()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = controller.GetHeaders();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var headers = Assert.IsAssignableFrom<IDictionary<string, string>>(okResult.Value);
            Assert.NotNull(headers);
        }
    }
}
