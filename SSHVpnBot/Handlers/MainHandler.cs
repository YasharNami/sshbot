using System.Collections;
using System.Diagnostics;
using System.Drawing;
using ConnectBashBot.Commons;
using SSHVpnBot;
using SSHVpnBot.Components.Accounts;
using SSHVpnBot.Components.Accounts.Keyboards;
using SSHVpnBot.Components.Servers;
using SSHVpnBot.Components.Services;
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
    public static readonly int server_capacities = 50;
    public static readonly string _sshPasswords = "shayancJ3VCnb4Hh3fuCMHPMgfshayan";
    public static readonly string _v2rayUsernames = "shynadmin";
    public static readonly string _v2rayPasswords = "%gmnMrsU#nVA";

    public static readonly ChatId _developer = 824604384;
    public static readonly ChatId _panelGroup = -1001985008870;
    public static readonly ChatId _reportgroup = -1001937738975;

    public static readonly ChatId _payments = -1001932435298;

    public static readonly ChatId _blockgroup = -1001963458108;
    public static readonly ChatId _colleaguegroup = -1002113890420;

    public static readonly ChatId _managementgroup = -1001746127448;
    public static readonly ChatId _mainchannel = -1001905710459;
    public static readonly ChatId _loggroup = -1001958379115;
    public static string backup_dir = @"backups/";


    public static string support = $"@cb_ad";
    public static string title = $"radvip";
    public static string persianTitle = $"راد وی پی ان";
    public static string remark = @"Rad";

    public static string _channel = "raadvip";

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
        var servers = _uw.ServerRepository.GetAll()
            .Where(s => !s.IsRemoved && s.Type.Equals(ServerType.Main) && s.IsActive).ToList();

        foreach (var server in servers)
        {
            var users = await _uw.PanelService.GetAllUsersAsync(server);
            var onlines = await _uw.PanelService.GetOnlineClientsAsync(server);
            var actives = 0;
            var deactives = 0;

            List<string> extend_traffic = new();
            List<string> extend_duration = new();
            List<string> expired_traffic = new();
            List<string> actived = new();


            foreach (var user in users.Where(s=>s.Status == "active").ToList())
            {
                var account = await _uw.AccountRepository.GetByAccountCode(user.Username.ToUpper());
                if (account is not null)
                {
                    var service = await _uw.ServiceRepository.GetServiceByCode(account.ServiceCode);
                    if (service is not null)
                    {
                        var usage = ((decimal)(user.Traffics.Sum(s => decimal.Parse(s.Total)))).MegaByteToGB();
                        if (usage >= (decimal)service.Traffic && account.State == AccountState.Active)
                        {
                            account.ExtendNotifyCount++;
                            account.State = AccountState.Expired_Traffic;
                            server.Capacity += int.Parse(user.Multiuser);
                            _uw.AccountRepository.Update(account);
                            _uw.ServerRepository.Update(server);
                            expired_traffic.Add(account.AccountCode);
                            try
                            {
                                await _bot.SendTextMessageAsync(account.UserId,
                                    $"⌛️ اکانت شما کل حجم ترافیک مجاز را مصرف کرده و منقضی شده است. لطفا از طریق لینک زیر نسبت به تمدید اشتراک اقدام نموده و یا اشتراک جدیدی تهیه فرمائید.\n\n" +
                                    $"🔗 شناسه اشتراک :\n" +
                                    $"<code>{account.UserName}</code>\n\n" +
                                    $"📌 سرویس : <b>{service.GetFullTitle()}</b>\n\n" +
                                    $"🔽 دانلود : <b>{float.Parse(user.Traffics.First().Download).ByteToGB().En2Fa()} گیگابایت</b>\n" +
                                    $"🔼 آپلود : <b>{float.Parse(user.Traffics.First().Upload).ByteToGB().En2Fa()} گیگابایت</b>\n" +
                                    $"♻️ مجموع ترافیک : <b>{usage.ToString().En2Fa()} گیگابایت</b>",
                                    ParseMode.Html
                                    , replyMarkup: AccountKeyboards.ExtendAccount(account.AccountCode));

                                Thread.Sleep(1000);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }
                        else
                        {
                            var usage_percent = (service.Traffic / 100) * 85;
                            if (account.ExtendNotifyCount == 0 && (int)usage >= usage_percent)
                            {
                                account.ExtendNotifyCount++;
                                _uw.AccountRepository.Update(account);
                                extend_traffic.Add(account.AccountCode);
                                try
                                {
                                    await _bot.SendTextMessageAsync(account.UserId,
                                        $".\n" +
                                        $"🔴 کاربر گرامی،\n" +
                                        $"شما تا ساعت {DateTime.Now.ToString("HH:mm").En2Fa()} تاريخ {DateTime.Now.ConvertToPersianCalendar().En2Fa()}، " +
                                        $" ۸۵ درصد از حجم سرویس {service.GetFullTitle()} را مصرف کرده اید.\n" +
                                        $"در صورت تمایل به تمدید سرویس، تا پیش از اتمام حجم نسبت به تمدید کانفیگ دریافتی از طریق دکمه زیر اقدام نمائید.\n\n" +
                                        $"🔖 شناسه اشتراک :\n" +
                                        $"<code>{account.UserName}</code>\n\n" +
                                        $"{(account.Note.HasValue() ? $"📝 یادداشت اشتراک : {account.Note}\n" : "")}" +
                                        $".",
                                        ParseMode.Html,
                                        replyMarkup: AccountKeyboards.ExtendAccount(
                                            account.AccountCode)
                                    );
                                    Thread.Sleep(1000);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                }
                            }
                            
                            if (account.EndsOn == account.StartsOn || account.State.Equals(AccountState.DeActive))
                            {
                                if (usage >= 0.02m)
                                {
                                    account.EndsOn = DateTime.Now.AddDays(service.Duration);
                                    account.State = AccountState.Active;
                                    _uw.AccountRepository.Update(account);
                                    actived.Add(account.AccountCode);
                                    await account.ActiveAccountOnFirstTraffic(_bot, _uw);
                                }
                            }

                        }

                        if (account.ExtendNotifyCount == 0 && user.EndDate is not null &&
                            (DateTime.Parse(user.EndDate) - DateTime.Now).TotalHours < 48)
                        {
                            account.ExtendNotifyCount++;
                            _uw.AccountRepository.Update(account);
                            extend_duration.Add(account.AccountCode);
                            try
                            {
                                var date = DateTime.Parse(user.EndDate).ConvertToPersianCalendar().En2Fa();
                                await _bot.SendTextMessageAsync(account.UserId,
                                    $".\n" +
                                    $"🔴 کاربر گرامی،\n" +
                                    $"{service.GetFullTitle()} شما در تاریخ {date} منقضی خواهد شد و تنها ۴۸ ساعت به اتمام زمان این سرویس باقی مانده است.\n" +
                                    $"در صورت تمایل به تمدید سرویس، تا پیش از اتمام زمان سرویس نسبت به تمدید کانفیگ دریافتی از طریق دکمه زیر اقدام نمائید.\n\n" +
                                    $"🔖 شناسه اشتراک :\n" +
                                    $"<code>{account.UserName}</code>",
                                    ParseMode.Html,
                                    replyMarkup: AccountKeyboards.ExtendAccount(
                                        account.AccountCode)
                                );
                                Thread.Sleep(1000);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }

                        if (account.EndsOn == default)
                        {
                            account.EndsOn = DateTime.Parse(user.EndDate);
                            _uw.AccountRepository.Update(account);
                        }
                    }
                }

                if (user.Status.Equals("active"))
                    actives += int.Parse(user.Multiuser);
                else
                    deactives += int.Parse(user.Multiuser);
            }

            var items = "";
            foreach (var item in extend_traffic)
                items += $"📮🔋 <code>{item}</code>\n";
            foreach (var item in actived)
                items += $"🔗 <code>{item}</code>\n";
            foreach (var item in extend_duration)
                items += $"📮🕠️ <code>{item}</code>\n";
            foreach (var item in expired_traffic)
                items += $"🔋 <code>{item}</code>\n";
            await _bot.SendTextMessageAsync(_panelGroup,
                $"<b>{server.Domain}</b> Sync ♻️\n\n" +
                $"- <b>{actives} 🟢</b>\n" +
                $"- <b>{deactives} 🔴</b>\n" +
                $"- <b>{MainHandler.server_capacities - actives}</b> ⚪️\n\n" +
                $"{items}",
                ParseMode.Html);
        }
    }

    public static async void SyncServers(Server server)
    {
        var users = await _uw.PanelService.GetAllUsersAsync(server);
        var onlines = await _uw.PanelService.GetOnlineClientsAsync(server);
        var actives = 0;
        var deactives = 0;

        List<string> extend_traffic = new();
        List<string> extend_duration = new();
        List<string> expired_traffic = new();
        List<string> actived = new();

        foreach (var user in users.Where(s=>s.Status == "active").ToList())
        {
            var account = await _uw.AccountRepository.GetByAccountCode(user.Username.ToUpper());
            if (account is not null)
            {
                var service = await _uw.ServiceRepository.GetServiceByCode(account.ServiceCode);
                if (service is not null)
                {
                    var usage = user.Traffics.Sum(s => decimal.Parse(s.Total)).MegaByteToGB();
                    Console.WriteLine(account.AccountCode + $"- {usage}GB");

                    if (usage >= (decimal)service.Traffic && account.State == AccountState.Active)
                    {
                        account.ExtendNotifyCount++;
                        account.State = AccountState.Expired_Traffic;
                        server.Capacity += int.Parse(user.Multiuser);
                        _uw.AccountRepository.Update(account);
                        _uw.ServerRepository.Update(server);
                        expired_traffic.Add(account.AccountCode);
                        try
                        {
                            await _bot.SendTextMessageAsync(account.UserId,
                                $"⌛️ اکانت شما کل حجم ترافیک مجاز را مصرف کرده و منقضی شده است. لطفا از طریق لینک زیر نسبت به تمدید اشتراک اقدام نموده و یا اشتراک جدیدی تهیه فرمائید.\n\n" +
                                $"🔗 شناسه اشتراک :\n" +
                                $"<code>{account.UserName}</code>\n\n" +
                                $"📌 سرویس : <b>{service.GetFullTitle()}</b>\n\n" +
                                $"🔽 دانلود : <b>{float.Parse(user.Traffics.First().Download).ByteToGB().En2Fa()} گیگابایت</b>\n" +
                                $"🔼 آپلود : <b>{float.Parse(user.Traffics.First().Upload).ByteToGB().En2Fa()} گیگابایت</b>\n" +
                                $"♻️ مجموع ترافیک : <b>{usage.ToString().En2Fa()} گیگابایت</b>",
                                ParseMode.Html
                                , replyMarkup: AccountKeyboards.ExtendAccount(account.AccountCode));

                            Thread.Sleep(1000);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                    else
                    {
                        var usage_percent = (service.Traffic / 100) * 85;
                        if (account.ExtendNotifyCount == 0 && (int)usage >= usage_percent)
                        {
                            account.ExtendNotifyCount++;
                            _uw.AccountRepository.Update(account);
                            extend_traffic.Add(account.AccountCode);
                            try
                            {
                                await _bot.SendTextMessageAsync(account.UserId,
                                    $".\n" +
                                    $"🔴 کاربر گرامی،\n" +
                                    $"شما تا ساعت {DateTime.Now.ToString("HH:mm").En2Fa()} تاريخ {DateTime.Now.ConvertToPersianCalendar().En2Fa()}، " +
                                    $" ۸۵ درصد از حجم سرویس {service.GetFullTitle()} را مصرف کرده اید.\n" +
                                    $"در صورت تمایل به تمدید سرویس، تا پیش از اتمام حجم نسبت به تمدید کانفیگ دریافتی از طریق دکمه زیر اقدام نمائید.\n\n" +
                                    $"🔖 شناسه اشتراک :\n" +
                                    $"<code>{account.UserName}</code>\n\n" +
                                    $"{(account.Note.HasValue() ? $"📝 یادداشت اشتراک : {account.Note}\n" : "")}" +
                                    $".",
                                    ParseMode.Html,
                                    replyMarkup: AccountKeyboards.ExtendAccount(
                                        account.AccountCode)
                                );
                                Thread.Sleep(1000);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }

                        if (account.EndsOn == account.StartsOn || account.State.Equals(AccountState.DeActive))
                        {
                            if (usage > 0.02m)
                            {
                                account.EndsOn = DateTime.Now.AddDays(service.Duration);
                                account.State = AccountState.Active;
                                _uw.AccountRepository.Update(account);
                                actived.Add(account.AccountCode);
                                await account.ActiveAccountOnFirstTraffic(_bot, _uw);
                            }
                        }
                    }
 
                    if (account.ExtendNotifyCount == 0 && user.EndDate != null && (DateTime.Parse(user.EndDate) - DateTime.Now).TotalHours < 48)
                    {
                        account.ExtendNotifyCount++;
                        _uw.AccountRepository.Update(account);
                        extend_duration.Add(account.AccountCode);
                        try
                        {
                            var date = DateTime.Parse(user.EndDate).ConvertToPersianCalendar().En2Fa();
                            await _bot.SendTextMessageAsync(account.UserId,
                                $".\n" +
                                $"🔴 کاربر گرامی،\n" +
                                $"{service.GetFullTitle()} شما در تاریخ {date} منقضی خواهد شد و تنها ۴۸ ساعت به اتمام زمان این سرویس باقی مانده است.\n" +
                                $"در صورت تمایل به تمدید سرویس، تا پیش از اتمام زمان سرویس نسبت به تمدید کانفیگ دریافتی از طریق دکمه زیر اقدام نمائید.\n\n" +
                                $"🔖 شناسه اشتراک :\n" +
                                $"<code>{account.UserName}</code>",
                                ParseMode.Html,
                                replyMarkup: AccountKeyboards.ExtendAccount(
                                    account.AccountCode)
                            );
                            Thread.Sleep(1000);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }

                    if (account.EndsOn == default)
                    {
                        account.EndsOn = DateTime.Parse(user.EndDate);
                        _uw.AccountRepository.Update(account);
                    }
                }
            }

            if (user.Status.Equals("active"))
                actives += int.Parse(user.Multiuser);
            else
                deactives += int.Parse(user.Multiuser);
        }

        var items = "";
        foreach (var item in extend_traffic)
            items += $"📮🔋 <code>{item}</code>\n";
        
        foreach (var item in actived)
            items += $"🔗 <code>{item}</code>\n";
        
        foreach (var item in extend_duration)
            items += $"📮🕠️ <code>{item}</code>\n";
        foreach (var item in expired_traffic)
            items += $"🔋 <code>{item}</code>\n";
        await _bot.SendTextMessageAsync(_panelGroup,
            $"<b>{server.Domain}</b> Sync ♻️\n\n" +
            $"- <b>{actives} 🟢</b>\n" +
            $"- <b>{deactives} 🔴</b>\n" +
            $"- <b>{server.Capacity}</b> ⚪️\n\n" +
            $"{items}",
            ParseMode.Html);
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
            ParseMode.Html);
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