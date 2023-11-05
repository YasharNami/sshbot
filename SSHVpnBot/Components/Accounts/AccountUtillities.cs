using System.Globalization;
using ConnectBashBot.Commons;
using ConnectBashBot.Telegram.Handlers;
using SSHVpnBot.Components.Accounts.Keyboards;
using SSHVpnBot.Components.Orders;
using SSHVpnBot.Components.Servers;
using SSHVpnBot.Components.Services;
using SSHVpnBot.Components.Subscribers;
using SSHVpnBot.Repositories.Uw;
using SSHVpnBot.Services.Panel.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Components.Accounts;

public static class AccountUtillities
{
    public static async void SuccessfulExtend(this ITelegramBotClient bot, Server server, long chatId, Account account)
    {
        await bot.SendTextMessageAsync(chatId,
            $"🟢 اشتراک شما با موفقیت تمدید شد :\n\n" +
            $"🔖 شناسه اشتراک :\n" +
            $"<code>{account.AccountCode}</code>\n\n" +
            $"⌛️ شروع : <b>{account.StartsOn.ConvertToPersianCalendar().En2Fa()}</b>\n" +
            $"⌛️ پایان : <b>{account.EndsOn.ConvertToPersianCalendar().En2Fa()}</b>\n" +
            $"📱 تعداد دستگاه‌های مجاز : <b>{(account.LimitIp == 1 ? "یک دستگاه" : "دو دستگاه")}</b>\n\n" +
            $"🔖 شناسه : \n" +
            $"<code>{account.AccountCode}</code>\n\n" +
            $"🌐 SSH Host :\n" +
            $"- <code>{server.Domain}</code>\n" +
            $"🔗 SSH Port :\n" +
            $"- <code>{server.SSHPort}</code>\n" +
            $"🔗 Udpgw Port :\n" +
            $"- <code>{server.UdpgwPort}</code>\n" +
            $"👤 Username :\n" +
            $"- <code>{account.UserName}</code>\n" +
            $"🔐 Password :\n" +
            $"- <code>{account.Password}</code>\n\n" +
            $"🆔 @ConnectBash",
            ParseMode.Html);
    }

    public static async Task CreateManyClientOnServer(this ITelegramBotClient _bot, IUnitOfWork _uw, Server server,
        Service service, Order order,
        User user,
        long chatId, CallbackQuery callBackQuery)
    {
        var clients = new List<Account>();
        for (var i = 0; i < order.Count; i++)
        {
            var count = await _uw.AccountRepository.GetLastItemIdAsync();
            var code = Account.GenerateNewAccountCode(count);

            var email = Account.GenerateNewClientEmail(order);
            var password = Account.GenerateNewAccountPassword();

            var client = new CreateNewClientDto()
            {
                Email = email,
                Username = code.ToLower(),
                Password = password,
                Traffic = (int)service.Traffic,
                type_traffic = "gb",
                Multiuser = service.UserLimit,
                connection_start = 30,
                Desc = $"{order.UserId}"
            };

            server.Capacity -= service.UserLimit;

            var newAccount = new Account()
            {
                AccountCode = code,
                IsActive = true,
                State = AccountState.DeActive,
                Sold = false,
                Traffic = service.Traffic,
                StartsOn = DateTime.Now,
                EndsOn = DateTime.Now,
                Email = client.Email,
                CreatedOn = DateTime.Now,
                ServerCode = server.Code,
                ServiceCode = service.Code,
                OrderCode = order.TrackingCode,
                IsRemoved = false,
                UsedTraffic = 0,
                ExtendNotifyCount = 0,
                LimitIp = client.Multiuser,
                UserName = client.Username.ToLower(),
                Password = password,
                UserId = order.UserId
            };

            var result = await _uw.PanelService.CreateNewClientAsync(server, client);

            if (result is not null)
            {
                order.State = OrderState.Done;
                _uw.ServerRepository.Update(server);
                _uw.OrderRepository.Update(order);
                _uw.AccountRepository.Add(newAccount);
                clients.Add(newAccount);

                //Task.Run(() => MainHandler.SyncServers(server));
            }
        }
        var items = "";
        foreach (var client in clients)
            items += $"<code>{client.AccountCode}</code>\n";
        await _bot.SendTextMessageAsync(MainHandler._panelGroup,
            $"🔗 New Client Created On {server.Domain}\n" +
            $"🔖 Order : #{order.TrackingCode}\n" +
            $"👤 User : #U{order.UserId}\n" +
            $"🔗 Service #{service.Code}\n\n" +
            $"{items}", ParseMode.Html);
        

        Task.Run(() =>
        {
            foreach (var client in clients)
            {
                 _bot.SendClientMessageAsync(_uw, order.UserId, server, client, service);
                 Thread.Sleep(1000);
            }
             _bot.SendTextMessageAsync(MainHandler._panelGroup,
                $"همکار با موفقیت {clients.Count.En2Fa()} اشتراک ها را دریافت کرد.");
             Task.Run(() => MainHandler.SyncServers(server));
        });
        
           if (order.PaymentType != PaymentType.Wallet)
                    await _bot.UpdateApprovedReceptMessages(order, user, chatId, callBackQuery);
    }

    public static async Task CreateSingleClientOnServer(this ITelegramBotClient _bot, IUnitOfWork _uw, Server server,
        Service service, Order order,
        User user,
        long chatId, CallbackQuery callBackQuery)
    {
        var count = await _uw.AccountRepository.GetLastItemIdAsync();
        var code = Account.GenerateNewAccountCode(count);

        var email = Account.GenerateNewClientEmail(order);
        var password = Account.GenerateNewAccountPassword();

        var client = new CreateNewClientDto()
        {
            Email = email,
            Username = code.ToLower(),
            Password = password,
            Traffic = (int)service.Traffic,
            type_traffic = "gb",
            Multiuser = service.UserLimit,
            connection_start = 30,
            Desc = $"{order.UserId}"
        };

        server.Capacity -= service.UserLimit;

        var newAccount = new Account()
        {
            AccountCode = code,
            IsActive = true,
            State = AccountState.Active,
            Traffic = service.Traffic,
            StartsOn = DateTime.Now,
            EndsOn = DateTime.Now.AddDays(service.Duration),
            Email = client.Email,
            CreatedOn = DateTime.Now,
            ServerCode = server.Code,
            ServiceCode = service.Code,
            OrderCode = order.TrackingCode,
            IsRemoved = false,
            UsedTraffic = 0,
            ExtendNotifyCount = 0,
            LimitIp = client.Multiuser,
            UserName = client.Username.ToLower(),
            Password = password,
            UserId = order.UserId
        };

        var result = await _uw.PanelService.CreateNewClientAsync(server, client);

        if (result is not null)
        {
            await _bot.SendTextMessageAsync(MainHandler._panelGroup,
                $"🔗 New Client Created On {server.Domain}\n" +
                $"🔖 Order : #{order.TrackingCode}\n" +
                $"👤 User : #U{order.UserId}\n" +
                $"🔗 Service #{service.Code}\n\n" +
                $"👤 <code>{newAccount.AccountCode}</code>", ParseMode.Html);
            order.State = OrderState.Done;
            _uw.ServerRepository.Update(server);
            _uw.OrderRepository.Update(order);
            _uw.AccountRepository.Add(newAccount);

            await _bot.SendClientMessageAsync(_uw, order.UserId, server, newAccount, service);

            await _bot.SendTextMessageAsync(MainHandler._panelGroup, "کاربر با موفقیت کانفیگ ها را دریافت کرد ✔️");

            if (order.PaymentType != PaymentType.Wallet)
                await _bot.UpdateApprovedReceptMessages(order, user, chatId, callBackQuery);

            Task.Run(() => MainHandler.SyncServers(server));
        }
    }

    public static string GetAccountStateEmoji(this Account account)
    {
        switch (account.State)
        {
            case AccountState.Active:
                return "🟢";
            case AccountState.DeActive:
                return "🔴";
            case AccountState.Expired_Traffic:
                return "🔋";
            case AccountState.Expired:
                return "⌛️";
            case AccountState.Blocked:
                return "❌";
            case AccountState.Blocked_Ip:
                return "📍";
            default:
                return "";
        }
    }

    public static async Task AdminAccountInfo(this ITelegramBotClient bot, IUnitOfWork uw, long chatId, Server server,
        PanelClientDto client, Account? account, Service? service,
        Order? order)
    {
        string order_Info = "", service_info = "";

        if (order is not null) order_Info = $"🔖 شماره سفارش : <b>#{order.TrackingCode}</b>\n";

        await bot.SendTextMessageAsync(chatId,
            $"<b>اطلاعات اشتراک :</b>\n\n" +
            $"🔖 شناسه : \n" +
            $"<code>{account.AccountCode}</code>\n\n" +
            $"🌐 SSH Host :\n" +
            $"- <code>{server.Domain}</code>\n" +
            $"🔗 SSH Port :\n" +
            $"- <code>{server.SSHPort}</code>\n" +
            $"🔗 Udpgw Port :\n" +
            $"- <code>{server.UdpgwPort}</code>\n" +
            $"👤 Username :\n" +
            $"- <code>{account.UserName}</code>\n" +
            $"🔐 Password :\n" +
            $"- <code>{account.Password}</code>\n\n" +
            $"🔽 دانلود : <b>{int.Parse(client.Traffics.First().Download).MegaByteToGB().En2Fa()} گیگابایت</b>\n" +
            $"🔼 آپلود : <b>{int.Parse(client.Traffics.First().Upload).MegaByteToGB().En2Fa()} گیگابایت</b>\n" +
            $"♻️ مجموع ترافیک : <b>{(int.Parse(client.Traffics.First().Upload) + int.Parse(client.Traffics.First().Download)).MegaByteToGB().En2Fa()} گیگابایت </b>\n\n" +
            $"{(service is not null ? $"🔗 <b>{service.GetFullTitle()}</b>\n" : "")}" +
            $"{order_Info}" +
            (account is not null
                ? $"📍 تاریخ انقضا : <b>{account.EndsOn.ConvertToPersianCalendar().En2Fa()}</b>\n" +
                  $"🔹 وضعیت اشتراک : <b>{account.State.ToDisplay()}</b>\n\n" +
                  $"📝 یادداشت اشتراک : {account.Note}".En2Fa()
                : ""),
            ParseMode.Html, replyMarkup: AccountKeyboards.ConfigManagement(client, server, account));
    }

    public static async Task SellerAccountInfo(this ITelegramBotClient bot, IUnitOfWork uw, long chatId,
        Server server, Account? account,
        ClientStats client)
    {
        var service = await uw.ServiceRepository.GetServiceByCode(account.ServiceCode);
        await bot.SendTextMessageAsync(chatId,
            $"<b>اطلاعات اشتراک :</b>\n\n" +
            $"{(service is not null ? $"🧩 <b>{service.GetFullTitle()}</b>\n\n" : "")}" +
            $"🌐 SSH Host :\n" +
            $"- <code>{server.Domain}</code>\n" +
            $"🔗 SSH Port :\n" +
            $"- <code>{server.SSHPort}</code>\n" +
            $"🔗 Udpgw Port :\n" +
            $"- <code>{server.UdpgwPort}</code>\n" +
            $"👤 Username :\n" +
            $"- <code>{account.UserName}</code>\n" +
            $"🔐 Password :\n" +
            $"- <code>{account.Password}</code>\n\n" +
            $"🔽 دانلود :\n" +
            $"- <b>{int.Parse(client.Download).MegaByteToGB()} GB</b>\n" +
            $"🔼 آپلود :\n" +
            $"- <b>{int.Parse(client.Upload).MegaByteToGB()} GB</b>\n" +
            $"♻️ مجموع ترافیک : \n" +
            $"- <b>{(int.Parse(client.Upload) + int.Parse(client.Download)).MegaByteToGB()} GB</b>\n\n" +
            (
                account is not null
                    ? $"📍 تاریخ انقضا : <b>{(account.State == AccountState.DeActive ? "نامشخص" : account.EndsOn.ConvertToPersianCalendar())}</b>\n" +
                      $"🔹 وضعیت اشتراک : <b>{account.State.ToDisplay()}</b>\n" +
                      $"📝 یادداشت اشتراک : {account.Note}\n" +
                      $"وضعیت فروش : {(account.Sold ? "فروخته شده 🟢" : "فروخته نشده 🔴")}".En2Fa()
                    : ""
            ),
            ParseMode.Html,
            replyMarkup: AccountKeyboards.ReporSellertAccount(account));
    }

    public static async Task SubscriberAccountInfo(this ITelegramBotClient bot, long chatId, Server server,
        Account account,
        Service? service,
        ClientStats client)
    {
        await bot.SendTextMessageAsync(chatId,
            $".\n" +
            $"🔗 <b>اطلاعات اشتراک شما :</b>\n\n" +
            $"{(service is not null ? $"🧩 <b>{service.GetFullTitle()}</b>\n\n" : "")}" +
            $"🌐 SSH Host :\n" +
            $"- <code>{server.Domain}</code>\n" +
            $"🔗 SSH Port :\n" +
            $"- <code>{server.SSHPort}</code>\n" +
            $"🔗 Udpgw Port :\n" +
            $"- <code>{server.UdpgwPort}</code>\n" +
            $"👤 Username :\n" +
            $"- <code>{account.UserName}</code>\n" +
            $"🔐 Password :\n" +
            $"- <code>{account.Password}</code>\n\n" +
            $"🔽 دانلود :\n" +
            $"- <b>{int.Parse(client.Download).MegaByteToGB()} GB</b>\n" +
            $"🔼 آپلود :\n" +
            $"- <b>{int.Parse(client.Upload).MegaByteToGB()} GB</b>\n" +
            $"♻️ مجموع ترافیک : \n" +
            $"- <b>{(int.Parse(client.Upload) + int.Parse(client.Download)).MegaByteToGB()} GB</b>\n\n" +
            $"📍 تاریخ انقضا : \n" +
            $"- <b>{account.EndsOn.ConvertToPersianCalendar()}</b>\n" +
            $"🔹 وضعیت اشتراک : \n" +
            $"- <b>{account.State.ToDisplay()}</b>",
            ParseMode.Html,
            replyMarkup: AccountKeyboards.ReportAccount(account));
    }
    
    public static async Task SendClientMessageAsync(this ITelegramBotClient _bot,
        IUnitOfWork uw, long chatId, Server server, Account account, Service service)
    {
        await _bot.SendTextMessageAsync(chatId,
            $".\n" +
            $"🔖<code>#{account.AccountCode}</code> | شناسه اشتراک\n\n" +
            $"🔗 اشتراک : <b>{service.GetFullTitle()}</b>\n" +
            $"⌛️ شروع : <b>{account.StartsOn.ConvertToPersianCalendar().En2Fa()}</b>\n" +
            $"⌛️ پایان : <b>{account.EndsOn.ConvertToPersianCalendar().En2Fa()}</b>\n\n" +
            $"🌐 SSH Host :\n" +
            $"- <code>{server.Domain}</code>\n" +
            $"🔗 SSH Port :\n" +
            $"- <code>{server.SSHPort}</code>\n" +
            $"🔗 Udpgw Port :\n" +
            $"- <code>{server.UdpgwPort}</code>\n" +
            $"👤 Username :\n" +
            $"- <code>{account.UserName}</code>\n" +
            $"🔐 Password :\n" +
            $"- <code>{account.Password}</code>\n\n" +
            $"🆔 @{MainHandler._channel}",
            ParseMode.Html);
    }
    
    public static async Task SendCheckClientAsync(this ITelegramBotClient _bot,
        IUnitOfWork uw, Server server, Account account)
    {
        await _bot.SendTextMessageAsync(account.UserId,
            $".\n" +
            $"🧪 اشتراک تست یک روزه ۲۰۰ مگابایتی\n" +
            $"⌛️ تاریخ انقضاء : <b>{account.EndsOn.ConvertToPersianCalendar().En2Fa()}</b>\n\n" +
            $"🌐 SSH Host :\n" +
            $"- <code>{server.Domain}</code>\n" +
            $"🔗 SSH Port :\n" +
            $"- <code>{server.SSHPort}</code>\n" +
            $"🔗 Udpgw Port :\n" +
            $"- <code>{server.UdpgwPort}</code>\n" +
            $"👤 Username :\n" +
            $"- <code>{account.UserName}</code>\n" +
            $"🔐 Password :\n" +
            $"- <code>{account.Password}</code>\n\n" +
            $"🆔 @{MainHandler._channel}",
            ParseMode.Html);
    }

    public static async Task SendSellerClientMessageAsync(this ITelegramBotClient _bot,
        IUnitOfWork uw, long chatId, Server server, Account account, Service service)
    {
        await _bot.SendTextMessageAsync(chatId,
            $".\n" +
            $"🔖<code>#{account.AccountCode}</code> | شناسه اشتراک\n\n" +
            $"🔗 اشتراک : <b>{service.GetFullTitle()}</b>\n" +
            $"⌛️ شروع : <b>{account.StartsOn.ConvertToPersianCalendar().En2Fa()}</b>\n" +
            $"⌛️ پایان : <b>{account.EndsOn.ConvertToPersianCalendar().En2Fa()}</b>\n\n" +
            $"🌐 SSH Host :\n" +
            $"- <code>{server.Domain}</code>\n" +
            $"🔗 SSH Port :\n" +
            $"- <code>{server.SSHPort}</code>\n" +
            $"🔗 Udpgw Port :\n" +
            $"- <code>{server.UdpgwPort}</code>\n" +
            $"👤 Username :\n" +
            $"- <code>{account.UserName}</code>\n" +
            $"🔐 Password :\n" +
            $"- <code>{account.Password}</code>\n\n" +
            $"🆔 @{MainHandler._channel}",
            ParseMode.Html,replyMarkup:AccountKeyboards.SellerConfig(account));
    }
    
    public static async Task ActiveAccountOnFirstTraffic(this Account account, ITelegramBotClient _bot, IUnitOfWork _uw)
    {
        try
        {
            await _bot.SendTextMessageAsync(account.UserId,
                $".\n" +
                $"تاریخ انقضا برای شناسه کاربری \n" +
                $"🔖 <code>{account.AccountCode}</code>\n" +
                $"🕢 <b>{account.EndsOn.ConvertToPersianCalendar()}</b>\n" +
                $"📝 <b>{account.Note}</b>\n\n" +
                $" ثبت گردید.🟢",
                ParseMode.Html);
            Thread.Sleep(1000);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

}