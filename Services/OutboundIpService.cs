namespace SimpleDotnetService.Services
{
    public class OutboundIpService : IIpAddressService
    {
        private readonly ILogger<OutboundIpService> logger;
        private readonly IIpifyProxy ipifyProxy;

        public OutboundIpService(ILogger<OutboundIpService> logger, IIpifyProxy ipifyProxy)
        {
            this.logger = logger;
            this.ipifyProxy = ipifyProxy;
        }

        public async Task<string> GetOutboundIpAsync()
        {
            logger.LogInformation("Getting outbound IP address");

            try
            {
                var ipAddress = await ipifyProxy.GetIpAsync();
                logger.LogInformation("Retrieved outbound IP: {IpAddress}", ipAddress);
                return ipAddress;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve outbound IP address");
                throw;
            }
        }
    }
}
