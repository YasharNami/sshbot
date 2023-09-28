using System.Data.SqlClient;
using Dapper;
using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Accounts.Repository;

public class AccountRepository : BaseRepository<Account>, IAccountRepository
{
    public async Task<Account?> GetByAccountCode(string accountCode)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<Account>(
                $"select * from accounts where accountCode=@accountCode and isremoved=0",
                new { accountCode });
        }
    }

    public async Task<Account?> GetByEmailAddressAsync(string email)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<Account>(
                $"select * from accounts where Replace(email,'tcp.','')='{email.Replace("tcp.", "")}'");
        }
    }

    public async Task<List<Account>> GetLessThanWeekAccountsAsync()
    {
        using (var db = new SqlConnection(conString))
        {
            return
                (await db.QueryAsync<Account>
                ($"select * from accounts where endson between GETDATE() and DateAdd(DD,+7,GETDATE()) " +
                 $" and isremoved=0 and state={(int)AccountState.Active}"))
                .ToList();
        }
    }

    public async Task<List<Account>> GetTodayCheckAccounts(long userid)
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync<Account>(
                $"select * from accounts where cast(startsOn as Date) = cast(getdate() as Date) and type=1")).ToList();
        }
    }

    public async Task<List<Account>> GetMineAccountsAsync(long userId)
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync<Account>($"select * from accounts where userId=@userId and isremoved=0",
                    new { userId }))
                .ToList();
        }
    }

    public async Task<int?> GetMineCountAsync(long userId)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<int>($"select count(id) from accounts where userId=@userId",
                new { userId });
        }
    }

    public async Task<List<long>> GetActiveOnes()
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync<long>(
                $"select distinct userid from accounts where state=1 and type=0 and isremoved=0")).ToList();
        }
    }

    public async Task<Account> GetMineByclientIdAsync(Guid clientId, long chatId)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<Account>(
                $"select * from accounts where clientId=@clientId and userId=@chatId and isremoved=0",
                new { clientId, chatId });
        }
    }

    public async Task<bool> GotCheckBefore(long chatId)
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync(
                $"select userid from accounts where userid=@chatId and type={(int)AccountType.Check}",
                new { chatId })).Any();
        }
    }


    public async Task<Account?> GetByclientIdAsync(Guid clientId)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<Account?>(
                $"select * from accounts where clientId=@clientId and isremoved=0",
                new { clientId });
        }
    }

    public async Task<bool> ExistByEmailAsync(string email)
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync($"select userid from accounts where email=@email and isremoved=0",
                new { email })).Any();
        }
    }

    public async Task<Account?> GetMineByVlessUrlAsync(string url, long chatId)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<Account>(
                $"select * from accounts where url=@url and userId=@chatId and isremoved=0", new { url, chatId });
        }
    }


    public async Task<List<long>> GotTestUsers()
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync<long>($"select distinct userid from accounts where type=0")).ToList();
        }
    }

    public async Task<int> GetByDomainAsync(string domain)
    {
        using (var db = new SqlConnection(conString))
        {
            domain = $"'%{domain}%'";
            var qurey = $"select count(id) from accounts where url like {domain} and isremoved=0";
            return await db.QueryFirstOrDefaultAsync<int>(qurey);
        }
    }

    public async Task<int> TodayConfigs()
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<int>(
                $"select count(id) from accounts  where  cast(startson as Date) = cast(getdate() as Date) and type=0 and isremoved=0");
        }
    }

    public async Task<Account?> GetAccountByEmail(string email)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<Account>(
                $"select * from accounts where email=@email and isremoved=0",
                new { email });
        }
    }

    public async Task<List<Account>> GetAllOrdersByCode(string ordercode)
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync<Account>($"select * from accounts where ordercode=@ordercode and isremoved=0",
                new { ordercode })).ToList();
        }
    }

    public async Task<List<Account>> GetByServerCodeAsync(string servercode)
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync<Account>($"select * from accounts where servercode=@servercode and isremoved=0",
                new { servercode })).ToList();
        }
    }

    public async Task<List<Account>> GetByOrderCodeAsync(string orderCode)
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync<Account>(
                $"select * from accounts where orderCode=@orderCode and isremoved=0",
                new { orderCode })).ToList();
        }
    }

    public async Task<int> TodayTests()
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<int>(
                $"select count(id) from accounts  where cast(startson as Date) = cast(getdate() as Date)  and type=1");
        }
    }

    public async Task<int?> YesterdayTests()
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<int>(
                $"select count(id) from accounts  where startson >= dateadd(day,datediff(day,1,GETDATE()),0) " +
                $"and startson < dateadd(day,datediff(day,0,GETDATE()),0) and type=1");
        }
    }

    public async Task<int?> YesterdayConfigs()
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QuerySingleOrDefaultAsync<int>(
                $"select count(id) from accounts  where startson >= dateadd(day,datediff(day,1,GETDATE()),0) " +
                $"and startson < dateadd(day,datediff(day,0,GETDATE()),0) and type=0 and isremoved=0");
        }
    }

    public async Task<List<Account>> GetByAccountNote(long userid, string query)
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync<Account>(
                    $"select * from accounts where userid=@userid and Note like N'%{query}%'", new { userid }))
                .ToList();
        }
    }
}