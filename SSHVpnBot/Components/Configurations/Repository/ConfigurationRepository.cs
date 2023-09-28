using System.Data.SqlClient;
using Dapper;
using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Configurations.Repository;

public class ConfigurationRepository : BaseRepository<Configuration>, IConfigurationRepository
{
    public async Task<Configuration?> GetByTypeAsync(ConfigurationType type)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<Configuration>
                ($"select * from configurations where type={(int)type}");
        }
    }
}