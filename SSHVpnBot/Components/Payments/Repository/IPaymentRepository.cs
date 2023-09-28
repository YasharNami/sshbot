using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Payments.Repository;

public interface IPaymentRepository : IBaseRepository<Payment>
{
    Task<Payment?> GetPaymentByCodeAsync(string code);
    Task<int> TodayCharges();
    Task<decimal?> YesterdayCharges();
}