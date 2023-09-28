
using System.Data.SqlClient;
using Dapper;
using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.PaymentMethods.Repository;

public class PaymentMethodRepository : BaseRepository<PaymentMethod>, IPaymentMethodRepository
{
    public async Task<PaymentMethod> GetPaymentType(int id)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<PaymentMethod>($"select * from paymentmethods where id=@id",
                new { id });
        }
    }

    public async Task Disable(int id)
    {
        using (var db = new SqlConnection(conString))
        {
            await db.ExecuteAsync($"update paymentmethods set isactive=0 where id=@id", new { id });
        }
    }

    public async Task Enable(int id)
    {
        using (var db = new SqlConnection(conString))
        {
            await db.ExecuteAsync($"update paymentmethods set isactive=1 where id=@id"
                , new { id });
        }
    }
}