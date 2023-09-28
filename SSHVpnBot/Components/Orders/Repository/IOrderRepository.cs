using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Orders.Repository;

public interface IOrderRepository : IBaseRepository<Order>
{
    Task<Order?> GetByTrackingCode(string trackingCode);
    Task<bool> IsFirstOrderAsync(long chatId);
    Task<int?> GetMineCountAsync(long userId);
    Task<int?> GetMineOrdersCount(long userId);
    Task<int?> GetSellerOrdersCount(long userId);
    Task<List<Order>> GetMineOrders(long uUserId);
    Task<int> GetAllByDiscountNumber(string discountNumber);
    Task<bool> UseOffCodeBefore(long chatId, string discountNumber);
    Task<List<long>> NotGotDiscountBefore();
    Task<int> Today();
    Task<int?> Yesterday();
    Task<int> TodayIncome();
    Task<int> TodayExtends();
    Task<int?> SumOfTotalAmountsByReferral(string toString);
    Task<List<long>> FactorsNotPaid();
    Task<bool> AnyPaidOrder(long userid);
    Task<List<Order>> TodayNotPaids();
    Task<List<Order>> YesterdayNotPaids();
    Task<int?> YesterdayIncome();
    Task<int?> YesterdayExtends();
}