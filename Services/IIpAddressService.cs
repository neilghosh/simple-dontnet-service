namespace SimpleDotnetService.Services
{
    public interface IIpAddressService
    {
        Task<string> GetOutboundIpAsync();
    }
}
