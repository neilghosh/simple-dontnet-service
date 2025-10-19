namespace dev.Services
{
    public interface INameService
    {
        Task<string> GetNameAsync(string? providedName = null);
    }
}