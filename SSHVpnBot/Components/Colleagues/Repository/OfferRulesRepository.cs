using System.Data.SqlClient;
using Dapper;
using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Colleagues.Repository;
public class OfferRulesRepository : BaseRepository<OfferRule>, IOfferRulesRepository
{
    public async Task<OfferRule> GetByServiceCode(string serviceCode)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<OfferRule>(
                $"select * from offerrules where serviceCode=@serviceCode", new { serviceCode });
        }
    }
}