using ConnectBashBot.Commons;
using ConnectBashBot.Telegram.Handlers;
using SSHVpnBot.Components.Accounts.Keyboards;
using SSHVpnBot.Components.Base;
using SSHVpnBot.Components.Orders;
using SSHVpnBot.Components.Servers;
using SSHVpnBot.Components.Services;
using SSHVpnBot.Components.Subscribers;
using SSHVpnBot.Repositories.Uw;
using SSHVpnBot.Services.Panel.Models;
using SSHVpnBot.Telegram;
using SSHVpnBot.Telegram.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Components.Accounts.Handlers;

public class AccountCallbackHandler : QueryHandler
{
    public AccountCallbackHandler(ITelegramBotClient _bot, Update update,
        IUnitOfWork _uw,
        Subscriber subscriber, CancellationToken cancellationToken = default)
        : base(_bot, update, _uw, subscriber)
    {
    }

    public override async Task QueryHandlerAsync()
    {
        if (data.StartsWith("account*"))
        {
            Task.Run(async () =>
            {
                var account = await _uw.AccountRepository.GetByAccountCode(data.Split("*")[1]);
                var server = await _uw.ServerRepository.GetServerByCode(account.ServerCode);
                if (server.IsActive)
                {
                    var clients = await _uw.PanelService.GetAllUsersAsync(server);
                    var client = clients!.FirstOrDefault(s => s.Email.Equals(account.Email));
                    if (client is not null)
                    {
                        await _bot.Choosed(callBackQuery);
                        var service = await _uw.ServiceRepository.GetServiceByCode(account.ServiceCode);
                        if (subscriber.Role.Equals(Role.Colleague))
                            await _bot.SellerAccountInfo(_uw, user.Id, server, account, client.Traffics[0]);
                        else
                            await _bot.SubscriberAccountInfo(user.Id, server, account, service, client.Traffics[0]);
                    }
                    else
                    {
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "اشتراک مورد نظر یافت نشد.",
                            true);
                    }
                }
                else
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                        $"سرور شناسه شما غیرفعال شده است.\n" +
                        $"جهت پیگیری به پشتیبانی مراجعه نمایید.", true);
                }
            });
        }
        else if (data.StartsWith("extend*"))
        {
            var account = await _uw.AccountRepository.GetByAccountCode(data.Replace("extend*", ""));
            if (account.State != AccountState.DeActive)
            {
                var service = await _uw.ServiceRepository.GetServiceByCode(account.ServiceCode);
                if (service is not null && !service.IsRemoved)
                {
                    await _bot.Choosed(callBackQuery);

                    var payments = _uw.PaymentMethodRepository.GetAll().Where(s => s.IsActive).ToList();

                    var order = new Order()
                    {
                        Amount = service.Price,
                        TotalAmount = service.Price + new Random().Next(100, 999),
                        Count = 1,
                        Type = OrderType.Extend,
                        AccountCode = account.AccountCode,
                        State = OrderState.WaitingForPayment,
                        ServiceCode = service.Code,
                        UserId = user.Id,
                        TrackingCode = Order.GenerateNewTrackingCode(),
                        CreatedOn = DateTime.Now
                    };

                    if (subscriber.Role.Equals(Role.Colleague))
                    {
                        order.TotalAmount = service.SellerPrice + new Random().Next(100, 999);
                        order.Amount = service.SellerPrice;
                    }

                    await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
                    await _bot.ReviewOrder(user.Id, payments, service, order);

                    _uw.OrderRepository.Add(order);
                }
                else
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "سرویس مورد نظر یافت نشد", true);
                }
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "اشتراک مورد نظر غیرفعال می باشد.",
                    true);
            }
        }
        else if (data.StartsWith("page*"))
        {
            await _bot.Choosed(callBackQuery);
            var page = int.Parse(data.Replace("page*", ""));
            var orders = await _uw.OrderRepository.GetMineOrders(user.Id);
            var accounts = (await _uw.AccountRepository.GetMineAccountsAsync(subscriber.UserId)).Where(s =>
                s.Type != AccountType.Check).ToList();
            var actives = accounts.Where(s => s.State.Equals(AccountState.Active) && s.EndsOn > DateTime.Now)
                .Count();
            var colleague = await _uw.ColleagueRepository.GetByChatId(user.Id);
            await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);

            if (orders.Count > 0)
                await _bot.SendTextMessageAsync(user.Id,
                    $".\n" +
                    $"<b>📁 مدیریت سفارشات موفق</b>\n\n" +
                    $"🔖 تعداد سفارشات انجام شده : <b>{orders.Count.En2Fa()} عدد</b>\n" +
                    $"🔗 مجموع اشتراک دریافتی : <b>{orders.Sum(s => s.Count).En2Fa()} عدد</b>\n" +
                    $"🟢 تعداد اشتراک فعال : <b>{actives.En2Fa()} عدد</b>\n" +
                    $"💳 مجموع پرداختی : <b>{orders.Sum(s => s.TotalAmount).ToIranCurrency().En2Fa()} تومان</b>\n\n" +
                    $"🗂 صفحه <b>{page.En2Fa()}</b> از <b>{Math.Ceiling((decimal)accounts.Count() / 20).En2Fa()}</b>\n\n" +
                    $"📌 قصد مشاهده اطلاعات کدام اشتراک را دارید؟",
                    ParseMode.Html,
                    replyMarkup: AccountKeyboards.MineAccounts(accounts, colleague, page));
            else
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, $"سفارشی یافت نشد.🤔", true);
        }
        else if (data.StartsWith("removeconfg*"))
        {
            var account = await _uw.AccountRepository.GetByAccountCode(data.Split("*")[1]);
            if (account is not null)
            {
                if (!account.State.Equals(AccountState.Active) && !account.State.Equals(AccountState.DeActive))
                {
                    account.IsRemoved = true;
                    _uw.AccountRepository.Update(account);
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "کانفیگ با موفقیت حذف شد.✖️️️️️", true);
                    await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
                    await _bot.SendTextMessageAsync(user.Id,
                        $".\n" +
                        $"✖️️️️️ کانفیگ با شناسه زیر با موفقیت حذف شد :\n\n" +
                        $"🔗 <code>{account.UserName}</code>\n" +
                        $"{(account.Note.HasValue() ? $"📝 یادداشت : {account.Note}" : "")}",
                        ParseMode.Html);
                    await _bot.SendTextMessageAsync(MainHandler._panelGroup,
                        $".\n" +
                        $"حذف_کانفیگ توسط همکار رخ داد. ✖️️️️️\n\n" +
                        $"🔗 <code>{account.UserName}</code>\n" +
                        $"{(account.Note.HasValue() ? $"📝 یادداشت : {account.Note}" : "")}",
                        ParseMode.Html);
                }
                else
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                        $"اشتراکی که در وضعیت {account.State.ToDisplay()} قرار دارد قابلیت حذف شدن ندارد.", true);
                }
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "اشتراک مورد نظر یافت نشد", true);
            }
        }
        else if (data.StartsWith("reget*"))
        {
            var account = await _uw.AccountRepository.GetByAccountCode(data.Split("*")[1]);
            if (account.State == AccountState.Active || account.State == AccountState.DeActive)
                Task.Run(async () =>
                {
                    var server = await _uw.ServerRepository.GetServerByCode(account.ServerCode);
                });
            else
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                    $"وضعیت سرویس در حالت {account.State.ToDisplay()} قرار دارد.", true);
        }
        else if (data.StartsWith("sold*"))
        {
            var account = await _uw.AccountRepository.GetByAccountCode(data.Split("*")[1]);
            if (account is not null)
            {
                if (!account.Sold)
                {
                    account.Sold = true;
                    _uw.AccountRepository.Update(account);
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "اشتراک با موفقیت فروخته شد.", true);
                    await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
                }
                else
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "اشتراک مورد فروخته شده است.", true);
                }
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "اشتراک مورد نظر یافت نشد.", true);
            }
        }
        else if (data.StartsWith("note*"))
        {
            var account = await _uw.AccountRepository.GetByAccountCode(data.Replace("note*", ""));
            if (account is null)
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "اشتراک مور نظر یافت نشد.", true);
            }
            else
            {
                await _bot.SendTextMessageAsync(user.Id,
                    ".\n" +
                    "📝 <b>در حال تنظیم یادداشت برای :</b>\n" +
                    $"🔖 <code>{account.UserName}</code>\n\n" +
                    "🗒 لطفا یک نام (یادداشت) برای کانفیگ  خود وارد کنید، مثلا اسم، فامیل یا شماره‌ تلفن مشتری می‌تواند انتخاب خوبی باشد.\n\n" +
                    "این اسم به شما کمک می‌کند تا بعدا بتوانید این کانفیگ را راحت‌تر از لیست کانفیگ‌های خود دریافت کنید. سعی کنید از نام‌های تکراری استفاده نکنید. (این نام به مشتری نشان داده نمی‌شود)"
                    , ParseMode.Html,
                    replyToMessageId: callBackQuery.Message.MessageId,
                    replyMarkup: MarkupKeyboards.Cancel());

                _uw.SubscriberRepository.ChangeStep(user.Id,
                    $"{Constants.AccountConstants}-note*{account.AccountCode}");
            }
        }
        else if (data.StartsWith("cnf*"))
        {
            var section = data.Split("*")[1];
            var account_code = data.Split("*")[2];
            var serverId = int.Parse(data.Split("*")[3]);

            var server = _uw.ServerRepository.GetById(serverId);

            var account = await _uw.AccountRepository.GetByAccountCode(account_code);
            var service = await _uw.ServiceRepository.GetServiceByCode(account.ServiceCode);
            if (account is not null)
                switch (section)
                {
                    case "reset":

                        break;
                    case "rm":
                        await _bot.SendTextMessageAsync(groupId,
                            $"آیا از حذف کانفیگ به شرح فوق اطمینان دارید؟\n\n",
                            ParseMode.Html,
                            replyToMessageId: callBackQuery.Message!.MessageId,
                            replyMarkup:
                            AccountKeyboards.RemoveConfigConfirmation(server, account.AccountCode));
                        break;
                    case "extend":
                        var extend = await _uw.PanelService.ExtendClientAsync(server, new ExtendClientDto()
                        {
                            re_date = DateTime.Now.AddDays(service!.Duration).ToString("yyyy-MM-dd"),
                            day_date = service.Duration.ToString(),
                            re_traffic = service.Traffic.ToString(),
                            username = account.UserName
                        });
                        if (extend is not null && extend!.message!.Equals("User Renewal"))
                        {
                            await _bot.DeleteMessageAsync(groupId, callBackQuery.Message.MessageId);
                            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "کانفیگ با موفقیت تمدید شد.", true);
                            account.EndsOn = DateTime.Now.AddDays(service.Duration);
                            _uw.AccountRepository.Update(account);
                            var users = await _uw.PanelService.GetAllUsersAsync(server);
                            var client = users!.FirstOrDefault(s => s.Username == account.UserName);
                            if (client is not null)
                            {
                                var order = await _uw.OrderRepository.GetByTrackingCode(account.OrderCode);
                                await _bot.AdminAccountInfo(_uw, groupId, server, client, account, service!, order!);
                            }
                            else
                            {
                                await _bot.SendTextMessageAsync(groupId, "اشتراک در پنل یافت نشد.", ParseMode.Html);
                            }
                        }
                        else
                        {
                            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "خطایی حین تمدید کانفیگ رخ داد.",
                                true);
                        }

                        break;
                    case "block":
                        var block = await _uw.PanelService.SuspendClientAsync(server, new SuspendClientDto()
                        {
                            Username = account.UserName
                        });

                        if (block is not null && block.message.Equals("User Deactivated"))
                        {
                            await _bot.DeleteMessageAsync(groupId, callBackQuery.Message.MessageId);
                            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "کانفیگ با موفقیت مسدود شد.", true);
                            account.State = AccountState.Blocked;
                            _uw.AccountRepository.Update(account);
                            var users = await _uw.PanelService.GetAllUsersAsync(server);
                            var client = users!.FirstOrDefault(s => s.Username == account.UserName);
                            if (client is not null)
                            {
                                var order = await _uw.OrderRepository.GetByTrackingCode(account.OrderCode);
                                await _bot.AdminAccountInfo(_uw, groupId, server, client, account, service!, order!);
                            }
                            else
                            {
                                await _bot.SendTextMessageAsync(groupId, "اشتراک در پنل یافت نشد.", ParseMode.Html);
                            }
                        }
                        else
                        {
                            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "خطایی حین مسدودسازی کانفیگ رخ داد.",
                                true);
                        }

                        break;
                    case "unblock":
                        var unblock = await _uw.PanelService.UnSuspendClientAsync(server, new UnSuspendClientDto()
                        {
                            Username = account.UserName
                        });

                        if (unblock is not null && unblock.message.Equals("User Activated"))
                        {
                            await _bot.DeleteMessageAsync(groupId, callBackQuery.Message.MessageId);
                            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "کانفیگ با آنبن شد.", true);
                            account.State = AccountState.Active;
                            _uw.AccountRepository.Update(account);
                            var users = await _uw.PanelService.GetAllUsersAsync(server);
                            var client = users!.FirstOrDefault(s => s.Username == account.UserName);
                            if (client is not null)
                            {
                                var order = await _uw.OrderRepository.GetByTrackingCode(account.OrderCode);
                                await _bot.AdminAccountInfo(_uw, groupId, server, client, account, service!, order!);
                            }
                            else
                            {
                                await _bot.SendTextMessageAsync(groupId, "اشتراک در پنل یافت نشد.", ParseMode.Html);
                            }
                        }
                        else
                        {
                            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "خطایی حین آنبن سازی کانفیگ رخ داد.",
                                true);
                        }

                        break;
                }
            else
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "اشترام مورد نظر یافت نشد.", true);
        }
        else if (data.StartsWith("rmconf*"))
        {
            var account = await _uw.AccountRepository.GetByAccountCode(data.Split("*")[1]);
            var server = _uw.ServerRepository.GetById(int.Parse(data.Split("*")[2]));
            if (account is not null)
            {
                if (server.IsActive)
                {
                    var remove = await _uw.PanelService.DeleteClientAsync(server, new DeleteClientDto()
                    {
                        Username = account.UserName
                    });

                    if (remove is not null)
                    {
                        await _bot.DeleteMessageAsync(groupId, callBackQuery.Message!.MessageId);
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "کانفیگ با موفقیت حذف شد.", true);
                        account.IsRemoved = true;
                        _uw.AccountRepository.Update(account);
                        await _bot.SendTextMessageAsync(MainHandler._panelGroup,
                            $"✖️️ Removed on {server.Domain}\n\n" +
                            $"<code>{account.AccountCode}</code>", ParseMode.Html);
                        await _bot.DeleteMessageAsync(groupId, callBackQuery.Message.ReplyToMessage.MessageId);
                        ;
                    }
                    else
                    {
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "خطایی حین مسدودسازی کانفیگ رخ داد.",
                            true);
                    }
                }
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "اشتراک مورد نظر یافت نشد.", true);
            }
        }
        else if (data.StartsWith("unblock*"))
        {
            var ansewr = data.Split("*")[1];
            var clientId = data.Split("*")[2];
            var serverId = int.Parse(data.Split("*")[3]);
            var port = int.Parse(data.Split("*")[4]);

            if (ansewr.Equals("approve"))
            {
                var account = await _uw.AccountRepository.GetByclientIdAsync(Guid.Parse(clientId));

                if (account.State.Equals(AccountState.Blocked) ||
                    account.State.Equals(AccountState.Blocked_Ip))
                {
                    var server = _uw.ServerRepository.GetById(serverId);
                }
                else
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                        $"وضعیت اشتراک مورد نظر {account.State.ToDisplay()} می باشد.", true);
                }
            }
            else
            {
                await _bot.DeleteMessageAsync(groupId, callBackQuery.Message.MessageId);
            }
        }
        else if (data.Equals("searchbynote"))
        {
            _uw.SubscriberRepository.ChangeStep(user.Id, $"{Constants.AccountConstants}-sendquery");
            var msg = await _bot.SendTextMessageAsync(user.Id,
                "🔍", replyMarkup: MarkupKeyboards.Cancel());
            await _bot.DeleteMessageAsync(user.Id, msg.MessageId);
            await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);

            await _bot.SendTextMessageAsync(user.Id,
                $".\n" +
                $"🔍 یادداشت کانفیگ مشتری یا بخشی از آن را را جهت بررسی وارد نمایید :\n\n" +
                $"به عنوان مثال :\n" +
                $"🔖 سینا محمدی",
                ParseMode.Html,
                replyMarkup: AccountKeyboards.SearchAccountByNote());
        }
        else if (data.Equals("searchbyuid"))
        {
            _uw.SubscriberRepository.ChangeStep(user.Id, $"{Constants.AccountConstants}-senduid");
            var msg = await _bot.SendTextMessageAsync(user.Id,
                "🔍", replyMarkup: MarkupKeyboards.Cancel());
            await _bot.DeleteMessageAsync(user.Id, msg.MessageId);
            await _bot.SendTextMessageAsync(user.Id,
                $".\n" +
                $"🔍 شناسه کانفیگ مشتری خود را جهت بررسی وارد نمایید :\n\n" +
                $"به عنوان مثال :\n" +
                $"🔖 88863817-cc4d-4f80-94d0-1e7bb0c6a7c7"
                , ParseMode.Html,
                replyMarkup: AccountKeyboards.SearchAccount());
        }
    }
}