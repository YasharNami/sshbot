using System.Data.SqlClient;
using Dapper;
using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Colleagues.Repository;

public class ColleagueRepository : BaseRepository<Colleague>, IColleagueRepository
{
    private IColleagueRepository _colleagueRepositoryImplementation;

    public async Task<Colleague?> GetByChatId(long chatId)
    {
        using (var db = new SqlConnection(conString))
        {
            return db.QuerySingleOrDefault<Colleague>($"select * from colleagues where userid=@chatid", new { chatId });
        }
    }

    public async Task<bool> RemarkIsExist(string remark)
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync($"select id from colleagues where tag=@remark", new { remark })).Any();
        }
    }

    public async Task<int> Today()
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<int>(
                $"select count(id) from colleagues where  cast(joinedon as Date) = cast(getdate() as Date) ");
        }
    }

    public async Task<int?> Yesterday()
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<int>(
                $"select count(id) from colleagues  where joinedon >= dateadd(day,datediff(day,1,GETDATE()),0) " +
                $"AND joinedon < dateadd(day,datediff(day,0,GETDATE()),0)");
        }
    }
}