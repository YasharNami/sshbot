using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.AccountReports.Repository;

public interface IAccountReportRepository : IBaseRepository<AccountReport>
{
    Task<AccountReport?> GetReportByCodeAsync(string code);
    Task<List<AccountReport>> GetAllByServerCodeAsync(string code);
    Task<List<AccountReport>> GetAllOpenByServerCodeAsync(string code);
    Task<bool> AnyOpenReportByAccountCode(string code);
}