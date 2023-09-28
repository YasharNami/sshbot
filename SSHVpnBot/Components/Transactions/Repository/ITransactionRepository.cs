using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Transactions.Repository;

public interface ITransactionRepository : IBaseRepository<Transaction>
{
    Task<List<Transaction>> GetAllMineAsync(long userid);
    Task<long?> GetMineBalanceAsync(long userid);
    Task<Transaction?> GetByCodeAsync(string code);

    Task<long?> GetMineReferralBalance(long userid);

    Task<List<TransactionRepository.TopReferralModel>> GetTopTenRerfferals();
    Task<bool> AnyByTypeAndUserId(long uUserId, TransactionType apology);
}