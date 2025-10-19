using Microsoft.AspNetCore.Mvc;
using dev.Services;

namespace dev.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NameController : ControllerBase
    {
        private readonly INameService nameService;
        private readonly ILogger<NameController> logger;

        public NameController(INameService nameService, ILogger<NameController> logger)
        {
            this.nameService = nameService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetName([FromQuery] string? name = null)
        {
            try
            {
                logger.LogInformation("Received request to get name with parameter: {Name}", name);
                
                var result = await nameService.GetNameAsync(name);
                
                return Ok(new { name = result });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while getting name");
                return StatusCode(500, new { error = "An error occurred while processing the request" });
            }
        }
    }
}
