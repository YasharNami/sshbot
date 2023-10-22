using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Servers.Repository;

public interface IServerRepository : IBaseRepository<Server>
{
    Task<Server?> GetServerByCode(string s);
    Task<List<Server>> GetAllByPaginationAsync(int page, int page_size);
    bool AnyByUrl(string serverCode, string utl);
    Task<Server> GetActiveOne(int spaceNeeded);
    Task<Server> GetTestServer();
    Task<bool> Capacity(int neededCapacity);
    bool AnyByDomain(string servervCode, string domain);
    Task<Server?> GetByDomainAsync(string domain);
}