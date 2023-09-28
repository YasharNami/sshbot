using System.Data.SqlClient;
using Dapper;
using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Orders.Repository;

public class OrderRepository : BaseRepository<Order>, IOrderRepository
{
    public async Task<Order?> GetByTrackingCode(string trackingCode)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<Order>($"select * from orders where trackingCode=@trackingCode",
                new { trackingCode });
        }
    }

    public async Task<bool> IsFirstOrderAsync(long chatId)
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync($"select id from orders where userId=@chatId", new { chatId })).Any();
        }
    }

    public async Task<int?> GetSellerOrdersCount(long userId)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<int?>(
                $"select sum(count) from orders where userId=@userId and state=2",
                new { userId });
        }
    }

    public async Task<List<Order>> GetMineOrders(long userId)
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync<Order>(
                $"select * from orders where userId=@userId and state=2 order by createdOn asc",
                new { userId })).ToList();
        }
    }
    public async Task<int?> GetMineOrdersCount(long userId)
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryFirstOrDefaultAsync<int?>($"select count(id) from orders where userId=@userId and state=2 order",
                new { userId }));
        }
    }

    public async Task<int> GetAllByDiscountNumber(string discountNumber)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<int>(
                $"select count(id) from orders where discountNumber=@discountNumber and state=2",
                new { discountNumber });
        }
    }

    public async Task<bool> UseOffCodeBefore(long chatId, string discountNumber)
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync(
                $"select id from orders where userId=@chatId and discountNumber=@discountNumber and state=2",
                new { chatId, discountNumber })).Any();
        }
    }

    public async Task<List<long>> NotGotDiscountBefore()
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync<long>($"select userid from orders where discountNumber is NULL and state=0"))
                .ToList();
        }
    }

    public async Task<int?> GetMineCountAsync(long userId)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<int?>(
                $"select count(id) from orders where userId=@userId and state=2", new { userId });
        }
    }

    public async Task<int> Today()
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<int>(
                $"select count(id) from orders  where cast(createdon as Date) = cast(getdate() as Date)  and state=2");
        }
    }

    public async Task<int?> Yesterday()
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<int>(
                $"select count(id) from orders  where createdon >= dateadd(day,datediff(day,1,GETDATE()),0) " +
                $"AND createdon < dateadd(day,datediff(day,0,GETDATE()),0) and state=2");
        }
    }

    public async Task<int> TodayIncome()
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<int>(
                $"select sum(totalamount) from orders  where  cast(createdon as Date) = cast(getdate() as Date) and state=2");
        }
    }

    public async Task<int> TodayExtends()
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<int>(
                $"select count(id) from orders  where  cast(createdon as Date) = cast(getdate() as Date) and state=2 and type=1");
        }
    }

    public async Task<int?> SumOfTotalAmountsByReferral(string referral)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<int?>(
                $"select sum(o.totalamount) from orders o inner join subscribers s on o.userid=s.userid where o.state=2 and s.referral=@referral",
                new { referral });
        }
    }

    public async Task<List<long>> FactorsNotPaid()
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync<long>($"SELECT DISTINCT(userid) FROM orders where state=0")).ToList();
        }
    }

    public async Task<bool> AnyPaidOrder(long userid)
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync($"select id from orders where userId=@userid and state=2", new { userid }))
                .Any();
        }
    }

    public async Task<List<Order>> TodayNotPaids()
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync<Order>(
                $"SELECT * FROM orders where cast(createdon as Date) = cast(getdate() as Date) and state=0")).ToList();
        }
    }

    public async Task<List<Order>> YesterdayNotPaids()
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync<Order>(
                $"select * from orders  where createdon >= dateadd(day,datediff(day,1,GETDATE()),0) " +
                $"AND createdon < dateadd(day,datediff(day,0,GETDATE()),0) and state=0")).ToList();
        }
    }

    public async Task<int?> YesterdayIncome()
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<int>(
                $"select sum(totalamount) from orders  where createdon >= dateadd(day,datediff(day,1,GETDATE()),0) " +
                $"AND createdon < dateadd(day,datediff(day,0,GETDATE()),0) and state=2");
        }
    }

    public async Task<int?> YesterdayExtends()
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<int>(
                $"select count(id) from orders  where  createdon >= dateadd(day,datediff(day,1,GETDATE()),0) " +
                $"AND createdon < dateadd(day,datediff(day,0,GETDATE()),0) and state=2 and type=1");
        }
    }
}