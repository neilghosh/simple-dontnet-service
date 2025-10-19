using Microsoft.AspNetCore.Mvc;
using SimpleDotnetService.Services;

namespace SimpleDotnetService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IpController : ControllerBase
    {
        private readonly IIpAddressService ipAddressService;
        private readonly ILogger<IpController> logger;

        public IpController(IIpAddressService ipAddressService, ILogger<IpController> logger)
        {
            this.ipAddressService = ipAddressService;
            this.logger = logger;
        }

        [HttpGet("outbound")]
        public async Task<IActionResult> GetOutboundIp()
        {
            try
            {
                logger.LogInformation("Received request to get outbound IP address");
                
                var ipAddress = await ipAddressService.GetOutboundIpAsync();
                
                return Ok(new { outboundip = ipAddress });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while getting outbound IP address");
                return StatusCode(500, new { error = "An error occurred while retrieving the outbound IP address" });
            }
        }
    }
}
