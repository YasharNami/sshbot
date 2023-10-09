using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Servers.Repository;

public interface IServerRepository : IBaseRepository<Server>
{
    Task<Server?> GetServerByCode(string s);
    Task<List<Server>> GetServersByCategoryCodeAsync(string code);
    Task<List<Server>> GetAllByPaginationAsync(int page, int page_size);
    bool AnyByUrl(string serverCode, string utl);
    Task<Server> GetActiveOne(int spaceNeeded);
    Task<Server?> GetActiveOneByCategoryCode(int spapce, string category);
    Task<Server> GetTestServer();
    Task<bool> Capacity(int neededCapacity, string category);
    bool AnyByDomain(string servervCode, string domain);
    Task<Server?> GetByDomainAsync(string domain);
}