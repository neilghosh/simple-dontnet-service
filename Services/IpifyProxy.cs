using System.Text.Json;

namespace SimpleDotnetService.Services
{
    public interface IIpifyProxy
    {
        Task<string> GetIpAsync();
    }

    public class IpifyProxy : IIpifyProxy
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<IpifyProxy> logger;
        private const string IpifyApiUrl = "https://api.ipify.org?format=json";

        public IpifyProxy(HttpClient httpClient, ILogger<IpifyProxy> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public async Task<string> GetIpAsync()
        {
            logger.LogInformation("Calling ipify API: {ApiUrl}", IpifyApiUrl);

            try
            {
                var response = await httpClient.GetStringAsync(IpifyApiUrl);
                var ipData = JsonSerializer.Deserialize<JsonElement>(response);
                var ipAddress = ipData.GetProperty("ip").GetString();

                logger.LogInformation("Successfully retrieved IP from ipify: {IpAddress}", ipAddress);
                return ipAddress ?? "Unknown";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to call ipify API");
                throw;
            }
        }
    }
}
