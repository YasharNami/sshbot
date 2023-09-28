using System.Data.SqlClient;
using Dapper;
using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Services.Repository;

public class ServiceRepository : BaseRepository<Service>, IServiceRepository
{
    public async Task<Service> GetServiceInfo(string id)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<Service>($"select * from services where id=@id", new { id });
        }
    }

    public async Task<List<Service>> GetServicesByCategoryCodeAsync(string code)
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync<Service>(
                $"select * from services where categorycode=@code and isremoved=0 and isactive=1",
                new { code })).ToList();
        }
    }

    public async Task<Service?> GetServiceByCode(string code)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<Service>($"select * from services where code=@code",
                new { code });
        }
    }


    public async Task<int> TodayTraffics()
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<int>(
                "select sum(s.traffic) from orders o INNER JOIN services s ON o.ServiceCode = s.Code where  cast(o.createdon as Date) = cast(getdate() as Date) and o.State=2");
        }
    }

    public async Task<int?> YesterdayTraffics()
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<int>(
                "select sum(s.traffic) from orders o INNER JOIN services s ON o.ServiceCode = s.Code where o.createdon >= dateadd(day,datediff(day,1,GETDATE()),0) " +
                $"AND  o.createdon < dateadd(day,datediff(day,0,GETDATE()),0) and o.State=2");
        }
    }

    public async Task<List<Service>> GetAllPublicsAsync()
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync<Service>(
                $"select * from services where isremoved=0 and type=0")).ToList();
        }
    }
}