using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Services.Repository;

public interface IServiceRepository : IBaseRepository<Service>
{
    Task<Service> GetServiceInfo(string id);
    Task<List<Service>> GetServicesByCategoryCodeAsync(string code);
    Task<Service?> GetServiceByCode(string code);
    Task<int> TodayTraffics();
    Task<int?> YesterdayTraffics();
    Task<List<Service>> GetAllPublicsAsync();
}