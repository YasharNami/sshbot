using System.Data.SqlClient;
using Dapper;
using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Subscribers.Repository;


public class SubscriberRepository : BaseRepository<Subscriber>, ISubscriberRepository
{
    public async Task<Subscriber?> GetByChatId(long chatId)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<Subscriber>($"select * from subscribers where userid=@chatid",
                new { chatId = chatId });
        }
    }

    public async Task<Subscriber?> GetByUserName(string username)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<Subscriber>($"select * from subscribers where username=@username",
                new { username });
        }
    }

    public void ChangeStep(long chatId, string step)
    {
        using (var db = new SqlConnection(conString))
        {
            db.Execute($"update subscribers set step=@step where Userid=@chatid"
                , new { Step = step, ChatId = chatId });
        }
    }

    public void UpdateReferal(long chatId, string referal)
    {
        using (var db = new SqlConnection(conString))
        {
            db.Execute($"update subscribers set referal=@referal where userid=@chatid"
                , new { Referal = referal, ChatId = chatId });
        }
    }


    public async Task<Role> GetRoleAsync(long userid)
    {
        using (var db = new SqlConnection(conString))
        {
            return await
                db.QuerySingleOrDefaultAsync<Role>($"select role from subscribers where userid=@userid"
                    , new { userid });
        }
    }

    public async Task<List<Subscriber>> GetReferralledAsync()
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync<Subscriber>(
                $"select * from subscribers where referral is not null")).ToList();
        }
    }

    public async Task<string?> CheckStep(long chatId)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<string?>($"select step from subscribers where userid=@chatid"
                , new { chatId });
        }
    }


    public bool HasToken(long chatId)
    {
        using (var db = new SqlConnection(conString))
        {
            return db.Query($"select accesstoken from subscribers where userid=@chatid and accesstoken is not null",
                new { chatId }).Any();
        }
    }


    public int GetCount()
    {
        using (var db = new SqlConnection(conString))
        {
            return db.QuerySingleOrDefault<int>($"select count(userid) from subscribers");
        }
    }

    public bool IsAdmin(long chatId)
    {
        using (var db = new SqlConnection(conString))
        {
            return db.Query(
                $"select userid from subscribers where userid=@chatid and role={(int)Role.Admin} or role={(int)Role.Owner}",
                new { chatId = chatId }).Any();
        }
    }

    public async Task<List<Subscriber>> GetAdmins()
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync<Subscriber>(
                $"select * from subscribers where role={(int)Role.Admin} or role={(int)Role.Owner}")).ToList();
        }
    }

    public async Task<List<Subscriber>> GetAllChatIds()
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync<Subscriber>(
                $"select fullname,userid,role,joinedon from subscribers order by joinedon desc")).ToList();
        }
    }

    public async Task<int?> GetAllByReferral(string referral)
    {
        using (var db = new SqlConnection(conString))
        {
            return db.QuerySingleOrDefault<int>($"select count(userid) from subscribers where referral=@referral",
                new { referral });
        }
    }

    public async Task<int> GetSellers()
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<int>($"select count(userid) from subscribers where role=1");
        }
    }

    public async Task<int> Today()
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<int>(
                $"select count(id) from subscribers  where cast(joinedon as Date) = cast(getdate() as Date) ");
        }
    }

    public async Task<int?> Yesterday()
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<int>(
                $"select count(id) from subscribers  where joinedon >= dateadd(day,datediff(day,1,GETDATE()),0) " +
                $"AND joinedon < dateadd(day,datediff(day,0,GETDATE()),0)");
        }
    }
}