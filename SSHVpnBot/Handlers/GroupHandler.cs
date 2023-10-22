using ConnectBashBot.Commons;
using SSHVpnBot.Components.Accounts;
using SSHVpnBot.Components.Colleagues;
using SSHVpnBot.Components.Orders;
using SSHVpnBot.Components.Orders.Keyboards;
using SSHVpnBot.Components.Servers;
using SSHVpnBot.Components.Servers.Keyboards;
using SSHVpnBot.Components.Services;
using SSHVpnBot.Components.Subscribers;
using SSHVpnBot.Components.Subscribers.Keyboards;
using SSHVpnBot.Components.Transactions;
using SSHVpnBot.Repositories.Uw;
using SSHVpnBot.Telegram.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ConnectBashBot.Telegram.Handlers;
public static class GroupHandler
{
    public static async Task HandleGroupMessageAsync(this ITelegramBotClient _bot, Update update, IUnitOfWork _uw,
        CancellationToken cancellationToken = default)
    {
        var messageType = update.Message!.Type;
        var message = update.Message;
        var user = update.Message.From!;
        var chatId = user.Id;
        var groupId = message.Chat.Id;

        var step = await _uw.SubscriberRepository.CheckStep(chatId);

        var userInfo = await _uw.SubscriberRepository.GetByChatId(message.From.Id);

        if (step == null)
        {
            var new_suscriber = await user.CreateSubscriberAsync();
            _uw.SubscriberRepository.AddWithId(new_suscriber);
            step = new_suscriber.Step;
        }


        if (messageType == MessageType.Text)
        {
            if (message.Text.Equals("/panel"))
            {
                var servers =  _uw.ServerRepository.GetAll();
                var onlines = await _uw.PanelService.GetOnlineClientsAsync(servers.First());
                await _bot.SendTextMessageAsync(groupId,
                    "قصد چه کاری را دارید؟",
                    replyMarkup: InlineKeyboards.AdminPanel());
            }
            else if (message.Text.Equals("/sync"))
            {
                Task.Run(() => { MainHandler.SyncAllServers(); });
            }
            else if (message.Text.StartsWith("/serverlist"))
            {
                var servers = _uw.ServerRepository.GetAll().Where(s => s.Type != ServerType.Check && !s.IsRemoved)
                    .ToList();
                var res = "";
                foreach (var server in servers)
                    res += $"{(server.IsActive ? "🟢" : "🔴")} <b>{server.Domain.Split(".")[0]}</b>\n" +
                           $"<code>{server.Url}</code>\n\n";

                await _bot.SendTextMessageAsync(groupId, res,parseMode: ParseMode.Html);
            }
            else if (message.Text.StartsWith("/server "))
            {
                
            }
            else if (message.Text.StartsWith("/order "))
            {
              
            }
            else if (message.Text.Equals("/today"))
            {
                if (userInfo.Role != Role.Subscriber && userInfo.Role != Role.Colleague)
                    await MainHandler.Today(groupId);
            }
            else if (message.Text.Equals("/yesterday"))
            {
                if (userInfo.Role != Role.Subscriber && userInfo.Role != Role.Colleague)
                    await MainHandler.Yesterday(groupId);
            }
            else if (message.Text.Equals("/week"))
            {
                if (userInfo.Role != Role.Subscriber && userInfo.Role != Role.Colleague)
                    await MainHandler.Today(groupId);
            }
              else if (message.Text.StartsWith("/user"))
                {
                    var userId = message.Text.Split(" ")[1];
                    var u = new Subscriber();
                    if (userId.StartsWith("@"))
                        u = await _uw.SubscriberRepository.GetByUserName(userId.Replace("@", ""));
                    else
                        u = await _uw.SubscriberRepository.GetByChatId(long.Parse(userId));

                    if (u is not null)
                        Task.Run(async () =>
                        {
                            var orders = await _uw.OrderRepository.GetMineOrders(u.UserId);
                            var accounts = await _uw.AccountRepository.GetMineAccountsAsync(u.UserId);

                            var activeAcc = accounts.Where(s => s.IsActive && s.EndsOn > DateTime.Now).ToList();
                            var checkAcc = accounts.FirstOrDefault(s => s.Type == AccountType.Check);

                            var totalPayments = 0m;
                            if (orders.Count > 0) totalPayments = orders.Sum(s => s.TotalAmount);

                            var balance = await _uw.TransactionRepository.GetMineBalanceAsync(u.UserId);
                            var referrals = await _uw.SubscriberRepository.GetAllByReferral(u.UserId.ToString());
                            Subscriber? referred = null;
                            if (u.Referral.HasValue() && u.Referral.IsNumber())
                            {
                                referred = await _uw.SubscriberRepository.GetByChatId(long.Parse(u.Referral));
                            }
                            Colleague? colleague = null;
                            if (u.Role.Equals(Role.Colleague))
                                colleague = await _uw.ColleagueRepository.GetByChatId(u.UserId);
                            await _bot.SendTextMessageAsync(groupId,
                                $"👤 اطلاعات کاربری : \n\n" +
                                $"🔖 شناسه کاربری :  #U{u.UserId}\n" +
                                $"✏️ نام و نام خانوادگی : <b>{u.FullName}</b>\n" +
                                $"{(u.Username.HasValue() ? $"✔️ نام کاربری : <b>@{u.Username}</b>\n" : "")}" +
                                $"🕚 تاریخ عضویت :\n" +
                                $"<b>{u.JoinedOn.ConvertToPersianCalendar().En2Fa()}</b>\n" +
                                $"{(referred is not null ? $"🗣 معرف : #<a href='tg://user?id={referred.UserId}'>{referred.UserId}</a>\n" : "")}" +
                                $"💂‍♀️ نقش کاربر : <b>{u.Role.ToDisplay()}</b> \n\n" +
                                $"💳 مجموع پرداختی تا کنون : <b>{totalPayments.ToIranCurrency().En2Fa()} تومان</b>\n" +
                                $"🛒 تعداد سفارشات تا کنون : <b>{orders.Count.ToString().En2Fa()}</b>\n" +
                                $"🔗 تعداد اشتراک های فعال : <b>{activeAcc.Count.ToString().En2Fa()}</b>\n" +
                                $"📌 اکانت تست : <b>{(checkAcc is null ? "دریافت نکرده است." : "دریافت کرده است.")}</b>\n\n" +
                                $"💰 موجودی کیف پول : <b>{((decimal)balance.Value).ToIranCurrency().En2Fa()} تومان</b>\n" +
                                $"👥 تعداد زیرمجموعه : <b>{referrals.Value.En2Fa()} نفر</b>",
                                ParseMode.Html,
                                replyMarkup:
                                SubscriberKeyboards.SingleUserManagement(u, colleague));
                        });
                    else
                        await _bot.SendTextMessageAsync(groupId, "کاربر مورد نظر یافت نشد.",
                            replyToMessageId: message.MessageId);
                }
         else if (message.Text.Equals("/referral"))
         {
             var referraled = await _uw.SubscriberRepository.GetReferralledAsync();

             var grouped = referraled.GroupBy(s => s.Referral).OrderByDescending(s=>s.Count()).ToList();
             Task.Run(async () =>
             {
                 foreach (var group in grouped)
                 {
                     if (group.Key.IsNumber())
                     {
                         var transactions = await _uw.TransactionRepository.GetMineReferralBalance(long.Parse(group.Key));
                         await _bot.SendTextMessageAsync(MainHandler._managementgroup,
                             $"👤 دعوت کننده : <code>U{group.Key}</code>\n" +
                             $"💰 درآمد کسب شده : <b>{transactions.Value.ToIranCurrency().En2Fa()} تومان</b>\n" +
                             $"👥 تعداد زیرمجموعه : <b>{group. Count().En2Fa()} زیرمجموعه</b>",
                             ParseMode.Html);
                         Thread.Sleep(5000);
                     }
                 }

             });
          
         }
            else if (message.Text.StartsWith("/charge_"))
            {
                var userId = message.Text.Split("_")[1];
                var amount = int.Parse(message.Text.Split("_")[2]);
                var u = await _uw.SubscriberRepository.GetByChatId(long.Parse(userId));
                if (u is not null)
                    Task.Run(async () =>
                    {
                        var code = Transaction.GenerateNewDiscountNumber();
                        var transaction = new Transaction()
                        {
                            Amount = amount,
                            CreatedOn = DateTime.Now,
                            TransactionCode = code,
                            UserId = u.UserId,
                            Type = TransactionType.Charge
                        };
                        _uw.TransactionRepository.Add(transaction);
                        var balance = await _uw.TransactionRepository.GetMineBalanceAsync(u.UserId);
                        if (amount > 0)
                        {
                            await _bot.SendTextMessageAsync(groupId,
                                $".\n" +
                                $"شارژ کیف پول کاربر با موفقیت انجام شد 🟢\n\n" +
                                $"🔖 <code>#{code}</code>\n" +
                                $"👤 <a href='tg://user?id={u.UserId}'>#U{u.UserId}</a>\n" +
                                $"💰 <b>{amount.ToIranCurrency().En2Fa()} تومان</b>\n" +
                                $"💰 موجودی کنونی : <b>{balance.Value.ToIranCurrency().En2Fa()} تومان</b>\n" +
                                $"🕑 <b>{transaction.CreatedOn.ConvertToPersianCalendar()} ساعت {transaction.CreatedOn.ToString("HH:mm").En2Fa()}</b>",
                                ParseMode.Html,
                                replyToMessageId: message.MessageId);

                            await _bot.SendTextMessageAsync(u.UserId,
                                $".\n" +
                                $"کیف پول شما به مبلغ <b>{amount.ToIranCurrency().En2Fa()} تومان</b> توسط مدیریت ربات شارژ شد.\n\n" +
                                $"💰 موجودی فعلی : {balance.Value.ToIranCurrency().En2Fa()} تومان",
                                ParseMode.Html);

                            await _bot.SendTextMessageAsync(MainHandler._payments,
                                $".\n" +
                                $"#شارژ_جدید صورت گرفت. 🟢\n\n" +
                                $"🔖 <code>#{code}</code>\n" +
                                $"👤 <a href='tg://user?id={u.UserId}'>#U{u.UserId}</a>\n" +
                                $"💰 <b>{amount.ToIranCurrency().En2Fa()} تومان</b>\n" +
                                $"💰 موجودی کنونی : <b>{balance.Value.ToIranCurrency().En2Fa()} تومان</b>\n" +
                                $"🕑 <b>{transaction.CreatedOn.ConvertToPersianCalendar()} ساعت {transaction.CreatedOn.ToString("HH:mm").En2Fa()}</b>\n\n" +
                                $"💂 شارژ توسط {user.FirstName} {user.LastName} صورت گرفت.",
                                ParseMode.Html);
                        }
                        else
                        {
                            await _bot.SendTextMessageAsync(groupId,
                                $".\n" +
                                $"کسری کیف پول کاربر با موفقیت انجام شد 🟢\n\n" +
                                $"🔖 <code>#{code}</code>\n" +
                                $"👤 <a href='tg://user?id={u.UserId}'>#U{u.UserId}</a>\n" +
                                $"💰 <b>{(-amount).ToIranCurrency().En2Fa()} تومان</b>\n" +
                                $"🕑 <b>{transaction.CreatedOn.ConvertToPersianCalendar()} ساعت {transaction.CreatedOn.ToString("HH:mm").En2Fa()}</b>",
                                ParseMode.Html,
                                replyToMessageId: message.MessageId);

                            await _bot.SendTextMessageAsync(u.UserId,
                                $".\n" +
                                $"کیف پول شما به مبلغ <b>{(-amount).ToIranCurrency().En2Fa()} تومان</b> توسط مدیریت کاهش یافت.\n\n" +
                                $"💰 موجودی فعلی : {balance.Value.ToIranCurrency().En2Fa()} تومان",
                                ParseMode.Html);

                            await _bot.SendTextMessageAsync(MainHandler._payments,
                                $".\n" +
                                $"#کسری_جدید صورت گرفت. 🟢\n\n" +
                                $"🔖 <code>#{code}</code>\n" +
                                $"👤 <a href='tg://user?id={u.UserId}'>#U{u.UserId}</a>\n" +
                                $"💰 <b>{(-amount).ToIranCurrency().En2Fa()} تومان</b>\n" +
                                $"🕑 <b>{transaction.CreatedOn.ConvertToPersianCalendar()} ساعت {transaction.CreatedOn.ToString("HH:mm").En2Fa()}</b>\n\n" +
                                $"💂 شارژ توسط {user.FirstName} {user.LastName} صورت گرفت.",
                                ParseMode.Html);
                        }
                    });
                else
                    await _bot.SendTextMessageAsync(groupId, "کاربر مورد نظر یافت نشد.",
                        replyToMessageId: message.MessageId);
            }
            
            else if (message.Text.StartsWith("/usage"))
            {
                Task.Run(async () =>
                {
                    var account =
                        await _uw.AccountRepository.GetByAccountCode(message.Text.Split(" ")[1]);
                    if (account is not null)
                    {
                        var server = await _uw.ServerRepository.GetServerByCode(account.ServerCode);
                        if (server!.IsActive)
                        {
                            var users = await _uw.PanelService.GetAllUsersAsync(server);
                            var client = users!.FirstOrDefault(s => s.Username == account.UserName);
                            if (client is not null)
                            {
                                Service? service = await _uw.ServiceRepository.GetServiceByCode(account.ServiceCode);
                                Order? order = await _uw.OrderRepository.GetByTrackingCode(account.OrderCode);
                                await _bot.AdminAccountInfo(_uw, groupId, server, client, account, service!, order!);
                            }
                            else await _bot.SendTextMessageAsync(groupId, "اشتراک در پنل یافت نشد.", ParseMode.Html, cancellationToken: cancellationToken);
                        }
                        else
                        {
                            await _bot.SendTextMessageAsync(groupId,
                                $"سرور <b>{server.Domain.Split(".")[0]}</b>  غیرفعال می باشد.",
                                ParseMode.Html,
                                replyToMessageId: message.MessageId);
                        }
                    }
                       
                });
            }
            else if (message.Text.StartsWith("/updateserver"))
            {
                var server = await _uw.ServerRepository.GetByDomainAsync(message.Text.Replace("/updateserver ", ""));
                if (server is null)
                {
                    await _bot.SendTextMessageAsync(groupId, "سرور یافت نشد", replyToMessageId: message.MessageId);
                }
                else
                {
                    if (server.Username.HasValue() && server.Domain != "تنظیم نشده" &&
                        server.Password != "تنظیم نشده" &&
                        server.Url.HasValue())
                        // if (!server.Session.HasValue())
                        // {
                        //     var login = await _uw.PanelService.Login(server);
                        //     if (login is null)
                        //     {
                        //         await _bot.DeActiveServer(_uw, server);
                        //         return;
                        //     }
                        //     else
                        //     {
                        //         server.Session = StringExtension.Encrypt(login.session);
                        //         _uw.ServerRepository.Update(server);
                        //     }
                        // }

                    await _bot.SendTextMessageAsync(chatId,
                        $"<b>اطلاعات سرور <code>#{server.Code}</code></b>\n\n" +
                        $"🔗 آدرس سرور :\n" +
                        $"<code>{server.Url}</code>\n" +
                        $"👤 نام کاربری : <b>{server.Username}</b>\n" +
                        $"📍 آدرس دامنه : <b>{server.Domain}</b>\n" +
                        $"👥 ظرفیت کاربر : <b>{server.Capacity} کاربر</b>\n",
                        ParseMode.Html,
                        replyMarkup: ServerKeyboards.SingleServerManagement(server));
                    await _bot.SendTextMessageAsync(groupId, "اطلاعات سرور مورد نظر جهت ویرایش برایتان ارسال شد.",
                        replyToMessageId: message.MessageId);
                }
            }
        }
    }
}