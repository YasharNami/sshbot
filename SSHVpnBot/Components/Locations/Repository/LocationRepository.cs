using System.Data.SqlClient;
using Dapper;
using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Locations.Repository;

public class LocationRepository : BaseRepository<Location>, ILocationRepository
{
    public async Task<Location?> GetLocationByCode(string code)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<Location>(
                $"select * from locations where code=@code", new { code });
        }
    }

    public async Task<bool> AnyByFlat(string flat)
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync($"select id from locations where flat=@flat", new { flat })).Any();
        }
    }
}