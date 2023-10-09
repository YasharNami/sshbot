using SSHVpnBot.Components.AccountReports.Repository;
using SSHVpnBot.Components.Accounts.Repository;
using SSHVpnBot.Components.Checkouts.Repository;
using SSHVpnBot.Components.Colleagues.Repository;
using SSHVpnBot.Components.Configurations.Repository;
using SSHVpnBot.Components.Discounts.Repository;
using SSHVpnBot.Components.Locations.Repository;
using SSHVpnBot.Components.Orders.Repository;
using SSHVpnBot.Components.PaymentMethods.Repository;
using SSHVpnBot.Components.Payments.Repository;
using SSHVpnBot.Components.Servers.Repository;
using SSHVpnBot.Components.ServiceCategories.Repository;
using SSHVpnBot.Components.Services.Repository;
using SSHVpnBot.Components.Subscribers.Repository;
using SSHVpnBot.Components.Transactions.Repository;
using SSHVpnBot.Services.Excel;
using SSHVpnBot.Services.Logger;
using SSHVpnBot.Services.Panel;

namespace SSHVpnBot.Repositories.Uw;

public interface IUnitOfWork
{
    ISubscriberRepository SubscriberRepository { get; }
    IPanelService PanelService { get; }

    IServiceRepository ServiceRepository { get; }
    IColleagueRepository ColleagueRepository { get; }
    IOrderRepository OrderRepository { get; }
    ILoggerService LoggerService { get; }
    IAccountReportRepository AccountReportRepository { get; }
    IPaymentMethodRepository PaymentMethodRepository { get; }
    IConfigurationRepository ConfigurationRepository { get; }
    IServerRepository ServerRepository { get; }
    IAccountRepository AccountRepository { get; }
    IServiceCategoryRepository ServiceCategoryRepository { get; }

    IDiscountRepository DiscountRepository { get; }
    ITransactionRepository TransactionRepository { get; }
    IPaymentRepository PaymentRepository { get; }
    IExcelService ExcelService { get; }
    ICheckoutRepository CheckoutRepository { get; }
    ILocationRepository LocationRepository { get; }
}