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
     public static async Task CreateSingleClientOnServer(this ITelegramBotClient _bot, IUnitOfWork _uw, Server server,
        Service service, Order order,
        User user,
        long chatId, CallbackQuery callBackQuery)
     {

         var code = Account.GenerateNewAccountCode();
         var email = Account.GenerateNewClientEmail(order);
         var password = Account.GenerateNewAccountPassword();
         
        var client = new CreateNewClientDto()
        {
            Email = email,
            Username = code,
            Password = password,
            Traffic = service.Traffic,
            type_traffic = "gb",
            Multiuser = service.UserLimit,
            ExpDate = DateTime.Now.AddDays(service.Duration),
            Desc = $"{order.UserId}"
        };
        
            server.Capacity -= service.UserLimit;
            
            var newAccount = new Account()
            {
                AccountCode =code,
                IsActive = true,
                State = AccountState.Active,
                Traffic = service.Traffic,
                StartsOn = DateTime.Now,
                EndsOn = DateTime.Now.AddDays(service.Duration),
                Email = client.Email,
                CreatedOn = DateTime.Now,
                ServerCode = server.Code,
                ServiceCode = server.Code,
                OrderCode = order.TrackingCode,
                IsRemoved = false,
                UsedTraffic = 0,
                ExtendNotifyCount = 1,
                LimitIp = client.Multiuser,
                UserName = client.Username,
                Password = code,
                
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

                await _bot.SendClientMessageAsync(_uw,order.UserId, newAccount,service);

                await _bot.SendTextMessageAsync(MainHandler._panelGroup, "کاربر با موفقیت کانفیگ ها را دریافت کرد ✔️");

                if (order.PaymentType != PaymentType.Wallet)
                    await _bot.UpdateApprovedReceptMessages(order, user, chatId, callBackQuery);

                //Task.Run(() => MainHandler.SyncServers(server));
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
    
        public static async Task AdminAccountInfo(this ITelegramBotClient bot,IUnitOfWork uw, long chatId, Server server,
        PanelClientDto client, Account? account, Service? service ,
        Order? order)
    {
       
        string order_Info = "", service_info = "";
        
        if (order is not null) order_Info = $"🔖 شماره سفارش : <b>#{order.TrackingCode}</b>\n";
        
        await bot.SendTextMessageAsync(chatId,
            $"<b>اطلاعات اشتراک :</b>\n\n" +
            $"🔖 شناسه : \n" +
            $"<code>{account.AccountCode}</code>\n" +
            $"{(account is not null ? $"🔗 اطلاعات اتصال : \n" + $"👤 <code>{account.UserName}</code>\n" +
                                      $"🔐 <code>{account.Password}</code>\n" : "\n")}" +
            $"🔽 دانلود : <b>{client.Stats.Sum(s=>float.Parse(s.Download)).ChangeDecimal(2).ToString(CultureInfo.InvariantCulture).En2Fa()} گیگابایت</b>\n" +
            $"🔼 آپلود : <b>{client.Stats.Sum(s=>float.Parse(s.Upload)).ChangeDecimal(2).ToString(CultureInfo.InvariantCulture).En2Fa()} گیگابایت</b>\n" +
            $"♻️ مجموع ترافیک : <b>{(client.Stats.Sum(s=>float.Parse(s.Download)).ChangeDecimal(2) + client.Stats.Sum(s=>float.Parse(s.Upload)).ChangeDecimal(2)).ToString(CultureInfo.InvariantCulture).En2Fa()} گیگابایت </b>\n\n" +
            $"{(service is not null ? $"🔗 <b>{service.GetFullTitle()}</b>\n" : "")}" +
            $"{order_Info}" +
            (account is not null
                ? $"📍 تاریخ انقضا : <b>{account.EndsOn.ConvertToPersianCalendar().En2Fa()}</b>\n" +
                  $"🔹 وضعیت اشتراک : <b>{account.State.ToDisplay()}</b>\n\n" +
                  $"📝 یادداشت اشتراک : {account.Note}".En2Fa()
                : ""),
            ParseMode.Html, replyMarkup: AccountKeyboards.ConfigManagement(client, server,account));
    }
    public static async Task SendClientMessageAsync(this ITelegramBotClient _bot,
        IUnitOfWork uw,long chatId, Account account,Service service)
    {

        await _bot.SendTextMessageAsync(chatId,
            $".\n" +
            $"🔖<code>#{account.AccountCode}</code> | شناسه اشتراک\n\n"+
            $"⌛️ شروع : <b>{account.StartsOn.ConvertToPersianCalendar().En2Fa()}</b>\n" +
            $"⌛️ پایان : <b>{account.EndsOn.ConvertToPersianCalendar().En2Fa()}</b>\n" +
            $"🔗 اشتراک : <b>{service.GetFullTitle()}</b>\n\n" +
            $"👤 <b>{account.UserName}</b>\n" +
            $"🔐 <b>{account.Password}</b>\n\n"+
            $"🆔 @{MainHandler._channel}",
            ParseMode.Html);
    }

}