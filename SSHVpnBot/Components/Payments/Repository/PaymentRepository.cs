using System.Data.SqlClient;
using Dapper;
using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Payments.Repository;

public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
{
    public async Task<Payment?> GetPaymentByCodeAsync(string paymentcode)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<Payment>($"select * from payments where paymentcode=@paymentcode",
                new { paymentcode });
        }
    }

    public async Task<int> TodayCharges()
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<int?>(
                       $"select sum(amount) from payments where state=1 and  cast(createdon as Date) = cast(getdate() as Date)") ??
                   0;
        }
    }

    public async Task<decimal?> YesterdayCharges()
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<decimal>(
                $"select sum(amount) from payments  where createdon >= dateadd(day,datediff(day,1,GETDATE()),0) " +
                $"AND createdon < dateadd(day,datediff(day,0,GETDATE()),0) and state=1");
        }
    }
}