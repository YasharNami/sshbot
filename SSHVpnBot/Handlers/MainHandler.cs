using System.Collections;
using System.Diagnostics;
using System.Drawing;
using ConnectBashBot.Commons;
using SSHVpnBot;
using SSHVpnBot.Components.Accounts;
using SSHVpnBot.Components.Servers;
using SSHVpnBot.Components.Transactions;
using SSHVpnBot.Repositories.Uw;
using SSHVpnBot.Telegram.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ConnectBashBot.Telegram.Handlers;

public class MainHandler
{
    private static int offset = 0;

    public static readonly string _db = "radmanvpndb";
    public static readonly bool is_develop = false;
    public static readonly int server_capacities = 100;
    public static readonly string _sshPasswords = "shayancJ3VCnb4Hh3fuCMHPMgfshayan";
    public static readonly string _v2rayUsernames = "shynadmin";
    public static readonly string _v2rayPasswords = "%gmnMrsU#nVA";

    public static readonly ChatId _developer = 824604384;
    public static readonly ChatId _panelGroup = -1001719099245;
    public static readonly ChatId _reportgroup = -1001937738975;

    public static readonly ChatId _payments = -1001746127448;
    
    public static readonly ChatId _blockgroup = -1001963458108;
    public static readonly ChatId _colleaguegroup = -1001678498821;

    public static readonly ChatId _managementgroup = -1001746127448;
    public static readonly ChatId _mainchannel = -1001905710459;
    public static readonly ChatId _loggroup = -1001958379115;
    public static string backup_dir = @"backups/";


    public static string support = $"@cb_ad";
    public static string title = $"radvip";
    public static string persianTitle = $"راد وی پی ان";
    public static string remark = @"Rad";

    public static string _channel = "rad_vip_channel";

    private static IUnitOfWork _uw;

    public static ITelegramBotClient _bot;

    public MainHandler(IUnitOfWork uw, ITelegramBotClient bot)
    {
        _uw = uw;
        _bot = bot;
    }

    public async void Run()
    {
        Console.WriteLine($"** Started {"Bot"} **");
        await _bot.SendTextMessageAsync(824604384, "Started");
        var offset = 0;
        while (true)
            try
            {
                var req = Program.Req;
                if (req < 14)
                {
                    var updates = await _bot.GetUpdatesAsync(offset, 14);

                    if (updates.Length != 0)
                        if (req < 14)
                            foreach (var update in updates)
                            {
                                req++;
                                offset = update.Id + 1;

                                if (update.Type != UpdateType.Message && update.Type != UpdateType.CallbackQuery)
                                    continue;
                                Console.WriteLine(
                                    $"Update Recived {update.Type.ToDisplay()} {DateTime.Now:yyyy/MM/ss HH:mm:ss}");

                                if (update.Type == UpdateType.CallbackQuery)
                                {
                                    await _bot.HandleCallbackQueryAsync(update, _uw);
                                    // await adminCallBackQueryHandler.Run(update);
                                }
                                else if (update.Type == UpdateType.Message)
                                {
                                    if (update.Message!.Chat.Type == ChatType.Group ||
                                        update.Message.Chat.Type == ChatType.Supergroup)
                                    {
                                        Console.Write($"{update.Message.Chat.Id} : {update.Message.Chat.Title}");
                                        await _bot.HandleGroupMessageAsync(update, _uw);
                                    }
                                    else if (update.Message.Chat.Type == ChatType.Private)
                                    {
                                        await _bot.HandlePrivateMessageAsync(update, _uw);
                                    }
                                }
                            }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    // await _bot.SendTextMessageAsync(
                    //     update.Message?.From!.Id ?? update.CallbackQuery!.From.Id, ex.Message,
                    //     cancellationToken: cancellationToken);
                    await Program.logger_bot.SendTextMessageAsync(
                        _loggroup!, ex.Message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
    }


    public static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };
        Console.WriteLine(errorMessage);

        return Task.CompletedTask;
    }
    

    public static async Task SyncAllServers()
    {
      
    }

    public static async void SyncServers(Server server)
    {
       
    }


   
    public static async Task Today(long chatId)
    {
        var users = await _uw.SubscriberRepository.Today();
        var colleagues = await _uw.ColleagueRepository.Today();
        var orders = await _uw.OrderRepository.Today();
        var notpaids = (await _uw.OrderRepository.TodayNotPaids()).Count;

        var income = await _uw.OrderRepository.TodayIncome();
        var extends = await _uw.OrderRepository.TodayExtends();

        var traffics = await _uw.ServiceRepository.TodayTraffics();
        var configs = await _uw.AccountRepository.TodayConfigs();
        var tests = await _uw.AccountRepository.TodayTests();

        var charges = await _uw.PaymentRepository.TodayCharges();

        await _bot.SendTextMessageAsync(chatId,
            $".\n<b>📌 گزارش آمار امروز :</b>\n" +
            $"🕓 {DateTime.Now.ConvertToPersianCalendar()} ساعت {DateTime.Now.ToString("HH:mm").En2Fa()}\n\n" +
            $"👥 تعداد کاربران : <b>{users.En2Fa()} کاربر</b>\n" +
            $"👨🏻‍💻 تعداد همکاران : <b>{colleagues.En2Fa()} همکار</b>\n" +
            $".\n" +
            $"🛒 تعداد سفارشات : <b>{orders.En2Fa()} سفارش</b>\n" +
            $"♻️ تعداد تمدیدها : <b>{extends.En2Fa()} تمدید</b>\n " +
            $"💸️ پرداخت نشده : <b>{notpaids.En2Fa()} فاکتور</b>\n " +
            $"➕ مجموع کل فروش : <b>{income.ToIranCurrency().En2Fa()} تومان</b>\n" +
            $"💰 مجموع کل شارژها : <b>{charges.ToIranCurrency().En2Fa() ?? 0.En2Fa()} تومان</b>\n" +
            $".\n" +
            $"🔗 تعداد کانفیگ ارسالی : <b>{configs.En2Fa()} کانفیگ</b>\n" +
            $"🧪 تعداد تست ارسالی : <b> {tests.En2Fa()} تست</b>\n" +
            $"🔋 مجموع ترافیک فروش : <b>{traffics.En2Fa()} گیگ</b>\n" +
            $".\n" +
            $"",
            parseMode:ParseMode.Html);
    }
    
    public static async Task Yesterday(long chatId)
    {
        var users = await _uw.SubscriberRepository.Yesterday();
        var colleagues = await _uw.ColleagueRepository.Yesterday();
        var orders = await _uw.OrderRepository.Yesterday();
        var notpaids = (await _uw.OrderRepository.YesterdayNotPaids()).Count;

        var income = await _uw.OrderRepository.YesterdayIncome();
        var extends = await _uw.OrderRepository.YesterdayExtends();

        var traffics = await _uw.ServiceRepository.YesterdayTraffics();
        var configs = await _uw.AccountRepository.YesterdayConfigs();
        var tests = await _uw.AccountRepository.YesterdayTests();

        var charges = await _uw.PaymentRepository.YesterdayCharges();

        await _bot.SendTextMessageAsync(chatId,
            $".\n<b>📌 گزارش آمار دیروز :</b>\n" +
            $"🕓 {DateTime.Now.AddDays(-1).ConvertToPersianCalendar()} ساعت {DateTime.Now.ToString("HH:mm").En2Fa()}\n\n" +
            $"👥 تعداد کاربران : <b>{users.Value.En2Fa()} کاربر</b>\n" +
            $"👨🏻‍💻 تعداد همکاران : <b>{colleagues.Value.En2Fa()} همکار</b>\n" +
            $".\n" +
            $"🛒 تعداد سفارشات : <b>{orders.Value.En2Fa()} سفارش</b>\n" +
            $"♻️ تعداد تمدیدها : <b>{extends.Value.En2Fa()} تمدید</b>\n " +
            $"💸️ پرداخت نشده : <b>{notpaids.En2Fa()} فاکتور</b>\n " +
            $"➕ مجموع کل فروش : <b>{income.Value.ToIranCurrency().En2Fa()} تومان</b>\n" +
            $"💰 مجموع کل شارژها : <b>{charges?.ToIranCurrency().En2Fa() ?? 0.En2Fa()} تومان</b>\n" +
            $".\n" +
            $"🔗 تعداد کانفیگ ارسالی : <b>{configs.Value.En2Fa()} کانفیگ</b>\n" +
            $"🧪 تعداد تست ارسالی : <b> {tests.Value.En2Fa()} تست</b>\n" +
            $"🔋 مجموع ترافیک فروش : <b>{traffics.Value.En2Fa()} گیگ</b>\n" +
            $".\n" +
            $"",
            ParseMode.Html);
    }
}