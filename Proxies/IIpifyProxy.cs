namespace SimpleDotnetService.Proxies
{
    public interface IIpifyProxy
    {
        Task<string> GetIpAsync();
    }
}
