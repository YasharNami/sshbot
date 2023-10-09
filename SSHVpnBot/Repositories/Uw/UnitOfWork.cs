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

public class UnitOfWork : IUnitOfWork
{
    private ISubscriberRepository _subscriberRepository;
    private IOrderRepository _orderRepository;
    private IColleagueRepository _colleagueRepository;
    private IServiceRepository _serviceRepository;
    private ILoggerService _loggerService;
    private IPaymentMethodRepository _paymentMethodRepository;
    private IConfigurationRepository _configurationRepository;
    private IServerRepository _serverRepository;
    private IAccountRepository _accountRepository;
    private IDiscountRepository _discountRepository;
    private ITransactionRepository _transactionRepository;
    private IPaymentRepository _paymentRepository;
    private ICheckoutRepository _checkoutRepository;
    private IExcelService _excelService;
    private ILocationRepository _locationRepository;
    private IServiceCategoryRepository _categoryRepository;
    private IAccountReportRepository _reportRepository;
    private IPanelService _panelService;
    public IPanelService PanelService
    {
        get
        {
            if (_panelService == null)
                _panelService = new PanelService();
            return _panelService;
        }
    }
    public IAccountReportRepository AccountReportRepository
    {
        get
        {
            if (_reportRepository == null)
                _reportRepository = new AccountReportRepository();
            return _reportRepository;
        }
    }
   
    public IServiceCategoryRepository ServiceCategoryRepository 
    {
        get
        {
            if (_categoryRepository == null)
                _categoryRepository = new ServiceCategoryRepository ();
            return _categoryRepository;
        }
    }

    public ILocationRepository LocationRepository
    {
        get
        {
            if (_locationRepository == null)
                _locationRepository = new LocationRepository();
            return _locationRepository;
        }
    }

    public IExcelService ExcelService
    {
        get
        {
            if (_excelService == null)
                _excelService = new ExcelService();
            return _excelService;
        }
    }

    public ICheckoutRepository CheckoutRepository
    {
        get
        {
            if (_checkoutRepository == null)
                _checkoutRepository = new CheckoutRepository();
            return _checkoutRepository;
        }
    }

    public IPaymentRepository PaymentRepository
    {
        get
        {
            if (_paymentRepository == null)
                _paymentRepository = new PaymentRepository();
            return _paymentRepository;
        }
    }
    
    public ITransactionRepository TransactionRepository
    {
        get
        {
            if (_transactionRepository == null)
                _transactionRepository = new TransactionRepository();
            return _transactionRepository;
        }
    }

    public ISubscriberRepository SubscriberRepository
    {
        get
        {
            if (_subscriberRepository == null)
                _subscriberRepository = new SubscriberRepository();
            return _subscriberRepository;
        }
    }

    public IDiscountRepository DiscountRepository
    {
        get
        {
            if (_discountRepository == null)
                _discountRepository = new DiscountRepository();
            return _discountRepository;
        }
    }

    public IOrderRepository OrderRepository
    {
        get
        {
            if (_orderRepository == null)
                _orderRepository = new OrderRepository();
            return _orderRepository;
        }
    }

    public IColleagueRepository ColleagueRepository
    {
        get
        {
            if (_colleagueRepository == null)
                _colleagueRepository = new ColleagueRepository();
            return _colleagueRepository;
        }
    }

    public IServiceRepository ServiceRepository
    {
        get
        {
            if (_serviceRepository == null)
                _serviceRepository = new ServiceRepository();
            return _serviceRepository;
        }
    }

    public ILoggerService LoggerService
    {
        get
        {
            if (_loggerService == null)
                _loggerService = new LoggerService();
            return _loggerService;
        }
    }

    public IPaymentMethodRepository PaymentMethodRepository
    {
        get
        {
            if (_paymentMethodRepository == null)
                _paymentMethodRepository = new PaymentMethodRepository();
            return _paymentMethodRepository;
        }
    }

    public IConfigurationRepository ConfigurationRepository
    {
        get
        {
            if (_configurationRepository == null)
                _configurationRepository = new ConfigurationRepository();
            return _configurationRepository;
        }
    }

    public IServerRepository ServerRepository
    {
        get
        {
            if (_serverRepository == null)
                _serverRepository = new ServerRepository();
            return _serverRepository;
        }
    }
    public IAccountRepository AccountRepository
    {
        get
        {
            if (_accountRepository == null)
                _accountRepository = new AccountRepository();
            return _accountRepository;
        }
    }
}