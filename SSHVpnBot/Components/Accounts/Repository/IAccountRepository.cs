using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Accounts.Repository;

public interface IAccountRepository : IBaseRepository<Account>
{
    Task<Account?> GetByAccountCode(string accountCode);
    Task<Account?> GetByEmailAddressAsync(string email);
    Task<List<Account>> GetTodayCheckAccounts(long userid);
    Task<List<Account>> GetLessThanWeekAccountsAsync();
    Task<List<Account>> GetMineAccountsAsync(long userId);
    Task<List<long>> GetActiveOnes();
    Task<int?> GetMineCountAsync(long userId);
    Task<Account> GetMineByclientIdAsync(Guid clientId, long chatId);
    Task<bool> GotCheckBefore(long chatId);
    Task<Account?> GetByclientIdAsync(Guid clientId);
    Task<bool> ExistByEmailAsync(string email);
    Task<Account?> GetMineByVlessUrlAsync(string messageText, long chatId);
    Task<List<long>> GotTestUsers();
    Task<int> GetByDomainAsync(string domain);
    Task<int> TodayTests();
    Task<int> TodayConfigs();
    Task<int?> YesterdayTests();
    Task<int?> YesterdayConfigs();

    Task<Account?> GetAccountByEmail(string clientEmail);
    Task<List<Account>> GetAllOrdersByCode(string ordercode);
    Task<List<Account>> GetByServerCodeAsync(string serverCode);
    Task<List<Account>> GetByOrderCodeAsync(string orderTrackingCode);
    Task<List<Account>> GetByAccountNote(long userid, string query);
    Task<int> GetLastItemIdAsync();
}