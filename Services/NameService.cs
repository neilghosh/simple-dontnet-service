namespace dev.Services
{
    public class NameService : INameService
    {
        private readonly ILogger<NameService> logger;
        private readonly string defaultName = "DefaultUser";

        public NameService(ILogger<NameService> logger)
        {
            this.logger = logger;
        }

        public async Task<string> GetNameAsync(string? providedName = null)
        {
            logger.LogInformation("Getting name with provided value: {ProvidedName}", providedName);
            
            // Simulate some async work (could be database call, API call, etc.)
            await Task.Delay(10);
            
            var result = !string.IsNullOrWhiteSpace(providedName) ? providedName : defaultName;
            
            logger.LogInformation("Returning name: {Name}", result);
            return result;
        }
    }
}