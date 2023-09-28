
using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.PaymentMethods.Repository;

public interface IPaymentMethodRepository : IBaseRepository<PaymentMethod>
{
    Task<PaymentMethod> GetPaymentType(int id);
    Task Disable(int id);

    Task Enable(int id);
}