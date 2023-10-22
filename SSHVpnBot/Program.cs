using ConnectBashBot.Telegram.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SSHVpnBot.Components.Colleagues.Repository;
using SSHVpnBot.Components.Orders.Repository;
using SSHVpnBot.Components.PaymentMethods.Repository;
using SSHVpnBot.Components.Services.Repository;
using SSHVpnBot.Components.Subscribers.Repository;
using SSHVpnBot.Jobs;
using SSHVpnBot.Repositories.Uw;
using SSHVpnBot.Services.Logger;
using Telegram.Bot;

var host = CreateHostBuilder(args).Build();
var scope = host.Services.CreateScope();
var bot = new TelegramBotClient("6617847105:AAHZzXqV3C8JoFXfVaRPidu8gebleBUKwYY");

var handler = new MainHandler(scope.ServiceProvider.GetRequiredService<IUnitOfWork>(), bot);
try
{
    thread = new Thread(new ThreadStart(handler.Run));
    var timer = new Timer(TimeHandler.ReqManager, null, 0, 1000);
    thread.Start();


    var serverSyncer = new ServerSyncer(TimeSpan.FromMinutes(15));
    serverSyncer.Start();
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}


await host.RunAsync();


static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, builder) => { builder.AddEnvironmentVariables(); })
        .ConfigureServices((_, services) =>
        {
            // var settings = _.Configuration.GetSection("TelegramBotSettings").Get<TelegramBotSettings>();
            // services.AddSingleton(settings);
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ILoggerService, LoggerService>();
            services.AddScoped<ISubscriberRepository, SubscriberRepository>();
            services.AddScoped<IColleagueRepository, ColleagueRepository>();
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        });
}

public partial class Program
{
    public static int Req = 0;
    public static Thread thread;
    public static TelegramBotClient logger_bot = new("6620566060:AAEOS_miPSf3LkcaRVpSUkb7sIjLCvIMp40");
    public static TelegramBotClient syncer_bot = new("6468788497:AAEGpZRuGhKQBZLvYvs6Oze_45z4RwXOVRc");
}