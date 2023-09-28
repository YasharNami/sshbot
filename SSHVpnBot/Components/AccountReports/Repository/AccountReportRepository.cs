using System.Data.SqlClient;
using Dapper;
using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.AccountReports.Repository;

public class AccountReportRepository : BaseRepository<AccountReport>, IAccountReportRepository
{
    public async Task<AccountReport?> GetReportByCodeAsync(string code)
    {
        using (var db = new SqlConnection(conString))
        {
            return await db.QueryFirstOrDefaultAsync<AccountReport>($"select * from accountreports where code=@code",
                new { code });
        }
    }

    public async Task<List<AccountReport>> GetAllOpenByServerCodeAsync(string code)
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync<AccountReport>(
                    $"select * from accountreports where servercode=@code and state={(int)ReportState.Open} and cast(createdon as Date) = cast(getdate() as Date)",
                    new { code }))
                .ToList();
        }
    }

    public async Task<List<AccountReport>> GetAllByServerCodeAsync(string code)
    {
        using (var db = new SqlConnection(conString))
        {
            return (await db.QueryAsync<AccountReport>(
                $"select * from accountreports where servercode=@code order by createdon desc", new { code })).ToList();
        }
    }

    public async Task<bool> AnyOpenReportByAccountCode(string code)
    {
        using (var db = new SqlConnection(conString))
        {
            return db.Query($"select code from accountreports where code=@code and state={(int)ReportState.Open} and  cast(createdon as Date) = cast(getdate() as Date)",
                new { code }).Any();
        }
    }
}