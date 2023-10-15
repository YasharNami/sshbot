using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Discounts.Repository;

public interface IDiscountRepository : IBaseRepository<Discount>
{
    Task<Discount?> GetByIdAsync(int parse);
    Task<bool> ExistByCode(string code, int id);
    Task<Discount?> GetByDiscountNumberAsync(string discountNumber);
    Task<Discount?> GetByCodeAsync(string messageText);
}