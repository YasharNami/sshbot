using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Locations.Repository;

public interface ILocationRepository : IBaseRepository<Location>
{
    Task<Location?> GetLocationByCode(string code);
    Task<bool> AnyByFlat(string flat);
}