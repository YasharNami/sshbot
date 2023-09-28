using System.Data.SqlClient;
using Dapper;
using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Transactions.Repository;

public class TransactionRepository : BaseRepository<Transaction>, ITransactionRepository
{
    public class TopReferralModel
    {
        public long UserId { get; set; }
        public int Count { get; set; }
        public float Total { get; set; }
    }

    public async Task<List<Transaction>> GetAllMineAsync(long userid)
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync<Transaction>(
                    $"select * from transactions where isremoved=0 and userid=@userid",
                    new { userid }))
                .ToList();
        }
    }

    public async Task<long?> GetMineBalanceAsync(long userid)
    {
        using (var db = new SqlConnection(conString))
        {
            var res = await db.QueryFirstOrDefaultAsync<long?>(
                $"select sum(amount) from transactions where isremoved=0 and userid=@userid",
                new { userid });
            return res == null ? 0 : res.Value;
        }
    }

    public async Task<Transaction?> GetByCodeAsync(string transactionCode)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<Transaction>(
                $"select * from transactions where isremoved=0 and transactionCode=@transactionCode",
                new { transactionCode });
        }
    }

    public async Task<long?> GetMineReferralBalance(long userid)
    {
        using (var db = new SqlConnection(conString))
        {
            var res = await db.QueryFirstOrDefaultAsync<long?>(
                $"select sum(amount) from transactions where isremoved=0 and userid=@userid and (type=3 or type=7)",
                new { userid });
            return res == null ? 0 : res.Value;
        }
    }

    public async Task<List<TopReferralModel>> GetTopTenRerfferals()
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync<TopReferralModel>(
                    $"select SUM(amount) as Total,UserId from transactions where type=3 group by userid  order by total desc"))
                .ToList();
        }
    }

    public async Task<bool> AnyByTypeAndUserId(long userid, TransactionType type)
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync(
                $"select id from transactions where userid=@userid and type={(int)type} and isremoved=0",
                new { userid })).Any();
        }
    }
}