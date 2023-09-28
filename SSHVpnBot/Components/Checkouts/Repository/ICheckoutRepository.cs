using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Checkouts.Repository;


public interface ICheckoutRepository : IBaseRepository<Checkout>
{
    Task<Checkout?> GetCheckoutByCode(string code);
}