using System.Data.SqlClient;
using Dapper;
using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Discounts.Repository;

public class DiscountRepository : BaseRepository<Discount>, IDiscountRepository
{
    public async Task<Discount?> GetByIdAsync(int id)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<Discount>($"select * from discounts where id=@id and isremoved=0",
                new { id });
        }
    }

    public async Task<bool> ExistByCode(string code, int id)
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync($"select code from discounts where code=@code and id!=@id and isremoved=0",
                new { code, id })).Any();
        }
    }

    public async Task<Discount> GetByDiscountNumberAsync(string discountNumber)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<Discount>(
                $"select * from discounts where discountNumber=@discountNumber and isremoved=0",
                new { discountNumber });
        }
    }

    public async Task<Discount?> GetByCodeAsync(string code)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<Discount>(
                $"select * from discounts where code=@code and isremoved=0",
                new { code });
        }
    }
}