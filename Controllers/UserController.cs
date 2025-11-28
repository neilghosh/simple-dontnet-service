using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SimpleDotnetService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> logger;

        public UserController(ILogger<UserController> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Gets the claims from the authenticated user's token.
        /// Requires a valid Azure AD bearer token.
        /// </summary>
        /// <returns>A dictionary of claims from the authenticated user's token</returns>
        [HttpGet("claims")]
        [Authorize]
        [ProducesResponseType(typeof(ClaimsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetClaims()
        {
            try
            {
                logger.LogInformation("Received request to get user claims");

                var claims = User.Claims
                    .GroupBy(c => c.Type)
                    .ToDictionary(
                        g => g.Key.Split('/').Last(),
                        g => g.Count() == 1 ? g.First().Value : string.Join(", ", g.Select(c => c.Value))
                    );

                var response = new ClaimsResponse
                {
                    IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                    Name = User.Identity?.Name,
                    Claims = claims,
                    Scopes = claims.ContainsKey("scp") ? claims["scp"].Split(' ').ToList() : new List<string>()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while getting user claims");
                return StatusCode(500, new { error = "An error occurred while retrieving user claims" });
            }
        }
    }

    public class ClaimsResponse
    {
        public bool IsAuthenticated { get; set; }
        public string? Name { get; set; }
        public Dictionary<string, string> Claims { get; set; } = new();
        public List<string> Scopes { get; set; } = new();
    }
}
