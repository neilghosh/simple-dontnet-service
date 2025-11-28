using Microsoft.AspNetCore.Mvc;

namespace SimpleDotnetService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<ConfigController> logger;

        public ConfigController(IConfiguration configuration, ILogger<ConfigController> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        /// <summary>
        /// Gets the Azure AD configuration for the SPA client.
        /// This endpoint is public and returns non-sensitive configuration.
        /// </summary>
        /// <returns>Azure AD configuration for the SPA client</returns>
        [HttpGet]
        [ProducesResponseType(typeof(AzureAdConfig), StatusCodes.Status200OK)]
        public IActionResult GetConfig()
        {
            try
            {
                logger.LogInformation("Received request to get Azure AD configuration");

                var clientId = configuration["AzureAd:ClientId"] ?? "";
                var tenantId = configuration["AzureAd:TenantId"] ?? "consumers";
                // Use FullScopes for the frontend (includes api:// prefix), fall back to building it
                var scopes = configuration["AzureAd:FullScopes"]?.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    ?? new[] { $"api://{clientId}/User.Read" };

                var config = new AzureAdConfig
                {
                    ClientId = clientId,
                    TenantId = tenantId,
                    Scopes = scopes.ToList()
                };

                return Ok(config);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while getting Azure AD configuration");
                return StatusCode(500, new { error = "An error occurred while retrieving configuration" });
            }
        }
    }

    public class AzureAdConfig
    {
        public string ClientId { get; set; } = "";
        public string TenantId { get; set; } = "consumers";
        public List<string> Scopes { get; set; } = new();
    }
}
