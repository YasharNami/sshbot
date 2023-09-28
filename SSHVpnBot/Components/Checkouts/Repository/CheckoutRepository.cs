using System.Data.SqlClient;
using Dapper;
using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Checkouts.Repository;

public class CheckoutRepository : BaseRepository<Checkout>, ICheckoutRepository
{
    public async Task<Checkout?> GetCheckoutByCode(string code)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<Checkout>(
                $"select * from checkouts where code=@code and isremoved=0",
                new { code });
        }
    }
}