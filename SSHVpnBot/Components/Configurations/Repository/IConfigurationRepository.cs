using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Configurations.Repository;

public interface IConfigurationRepository : IBaseRepository<Configuration>
{
    Task<Configuration?> GetByTypeAsync(ConfigurationType collleagueSide);
}