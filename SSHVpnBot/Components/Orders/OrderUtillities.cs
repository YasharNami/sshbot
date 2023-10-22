using ConnectBashBot.Commons;
using ConnectBashBot.Telegram.Handlers;
using SSHVpnBot.Components.Accounts;
using SSHVpnBot.Components.Discounts;
using SSHVpnBot.Components.Servers;
using SSHVpnBot.Components.Services;
using SSHVpnBot.Components.Subscribers;
using SSHVpnBot.Components.Transactions;
using SSHVpnBot.Repositories.Uw;
using SSHVpnBot.Services.Panel.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Components.Orders;

public static class OrderUtillities
{
    public static async Task UpdateApprovedReceptMessages(this ITelegramBotClient _bot, Order order, User user,
        long chatId, CallbackQuery callBackQuery)
    {
        await _bot.EditMessageTextAsync(MainHandler._payments,
            callBackQuery.Message!.MessageId,
            callBackQuery.Message.Text!.Replace(
                "♻️ آیا اطلاعات فوق را جهت تحویل سرویس تایید میکنید؟",
                "" +
                $"توسط {user.FirstName + " " + user.LastName} تایید شد ✅️"));
    }

    public static async Task UpdateDeclinedReceptMessages(this ITelegramBotClient _bot, Order order, User user,
        long chatId, CallbackQuery callBackQuery)
    {
        await _bot.SendTextMessageAsync(order.UserId,
            $"رسید سفارش شما با کد پیگیری <b>#{order.TrackingCode}</b> رد شد ✖️.\n" +
            $"جهت پیگیری علت به پشتیبانی مراجعه نمایید.",
            ParseMode.Html);

        await _bot.EditMessageTextAsync(MainHandler._payments,
            callBackQuery.Message.MessageId,
            callBackQuery.Message.Text.Replace("♻️ آیا اطلاعات فوق را جهت تحویل سرویس تایید میکنید؟",
                "" +
                $"توسط {user.FirstName + " " + user.LastName} رد شد✖️"));
    }

    public static async Task ApproveExtendOrder(this ITelegramBotClient _bot, IUnitOfWork _uw, Order order, User user,
        long chatId, CallbackQuery callBackQuery)
    {
        var service = await _uw.ServiceRepository.GetServiceByCode(order.ServiceCode);


        var account = await _uw.AccountRepository.GetByAccountCode(order.AccountCode);
        var server = await _uw.ServerRepository.GetServerByCode(account.ServerCode);
        if (server is not null)
            if (server.IsActive)
            {
                var users = await _uw.PanelService.GetAllUsersAsync(server);
                var client = users.FirstOrDefault(s => s.Username.Equals(order.AccountCode.ToLower()));
                if (client is not null)
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "پرداخت با موفقیت تایید شد.✅", true);


                    var extend = await _uw.PanelService.ExtendClientAsync(server, new ExtendClientDto()
                    {
                        re_date = DateTime.Now.AddDays(service.Duration).ToString("yyyy-MM-dd"),
                        day_date = service.Duration.ToString(),
                        re_traffic = service.Traffic.ToString(),
                        username = client.Username
                    });
                    if (extend is not null)
                    {
                        if (extend!.message!.Equals("User Renewal"))
                        {
                            order.State = OrderState.Done;

                            if (!account.State.Equals(AccountState.Expired))
                                account.EndsOn = DateTime.Now.AddDays(service.Duration);
                            else account.EndsOn = DateTime.Now.AddDays(service.Duration);

                            account.IsActive = true;
                            account.Traffic += service.Traffic;
                            account.State = AccountState.Active;

                            _uw.AccountRepository.Update(account);
                            _uw.OrderRepository.Update(order);

                            _bot.SuccessfulExtend(server, order.UserId, account);

                            if (order.PaymentType != PaymentType.Wallet)
                                await _bot.EditMessageTextAsync(MainHandler._payments,
                                    callBackQuery.Message.MessageId,
                                    callBackQuery.Message.Text.Replace(
                                        "♻️ آیا اطلاعات فوق را جهت تمدید سرویس تایید میکنید؟",
                                        "" +
                                        $"توسط {user.FirstName + " " + user.LastName} تایید شد ✅️"));

                            await _bot.SendTextMessageAsync(MainHandler._panelGroup,
                                $".\n" +
                                $"♻️ تمدید جدید اشتراک :\n\n" +
                                $"🌐 <b>{server.Domain}</b>\n" +
                                $"🧩 <b>{service.GetFullTitle()}</b>\n" +
                                $"🔗 <code>{account.AccountCode}</code>\n" +
                                $"🕧 {account.EndsOn.ConvertToPersianCalendar()}",
                                ParseMode.Html);

                            var subscriber = await _uw.SubscriberRepository.GetByChatId(order.UserId);
                            if (!subscriber.Role.Equals(Role.Colleague) && subscriber.Referral.HasValue())
                            {
                                long user_id = 0;
                                if (long.TryParse(subscriber.Referral, out user_id))
                                {
                                    var referral = await _uw.SubscriberRepository.GetByChatId(user_id);
                                    if (referral is not null)
                                        if (!referral.Role.Equals(Role.Colleague) && referral.IsActive)
                                        {
                                            var code = Transaction.GenerateNewDiscountNumber();
                                            var transaction = new Transaction()
                                            {
                                                Amount = order.TotalAmount / 100 * 10,
                                                CreatedOn = DateTime.Now,
                                                TransactionCode = code,
                                                UserId = referral.UserId,
                                                Type = TransactionType.OrderReward
                                            };
                                            _uw.TransactionRepository.Add(transaction);
                                            await _bot.SendTextMessageAsync(referral.UserId,
                                                $".\n" +
                                                $"👥 #پاداش_زیرمجموعه به شما تعلق گرفت.\n\n" +
                                                $"🔖 شناسه پاداش : <code>#{transaction.TransactionCode}</code>\n" +
                                                $"👤 شناسه زیرمجموعه : <code>#U{order.UserId}</code>\n" +
                                                $"💰 به مبلغ <b>{transaction.Amount.ToIranCurrency().En2Fa()} تومان</b>\n\n" +
                                                $"<b>{transaction.CreatedOn.ConvertToPersianCalendar().En2Fa()} ساعت {transaction.CreatedOn.ToString("HH:mm").En2Fa()}</b>\n" +
                                                $".",
                                                ParseMode.Html);
                                        }
                                }
                            }
                        }
                        else
                        {
                            await _uw.LoggerService.LogException(_bot, extend.message);
                        }
                    }
                }
                else
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "اشتراک مورد نظر یافت نشد.", true);
                }
            }
    }

    public static async Task ApproveNewOrder(this ITelegramBotClient _bot, IUnitOfWork _uw, Order order, User user,
        long chatId, CallbackQuery callBackQuery)
    {
        var service = await _uw.ServiceRepository.GetServiceByCode(order.ServiceCode);
        if (await _bot.CheckServerCapacityForOrder(_uw, order, service, callBackQuery))
        {
            order.State = OrderState.Done;
            var cli_counts = order.Count * service.UserLimit;

            var server = await _uw.ServerRepository.GetActiveOne(cli_counts);
            if (order.Count == 1)
                await _bot.CreateSingleClientOnServer(_uw, server, service, order, user, chatId, callBackQuery);
            else
                await _bot.CreateManyClientOnServer(_uw, server, service, order, user, chatId, callBackQuery);

            if (order.PaymentType == PaymentType.Wallet)
            {
                var code = Transaction.GenerateNewDiscountNumber();
                var transaction = new Transaction()
                {
                    Amount = order.TotalAmount - order.TotalAmount * 2,
                    CreatedOn = DateTime.Now,
                    TransactionCode = code,
                    UserId = order.UserId,
                    Type = TransactionType.Order
                };
                _uw.TransactionRepository.Add(transaction);

                await _bot.SendTextMessageAsync(order.UserId,
                    $".\n" +
                    $"پرداخت فاکتور شما با موفقیت انجام شد. 🟢\n\n" +
                    $"🔖 شناسه پرداخت : <code>#{transaction.TransactionCode}</code>\n" +
                    $"🔖 شناسه سفارش : <code>#{order.TrackingCode}</code>\n" +
                    $"🔗 <b>{order.Count.En2Fa()} اشتراک {service.GetFullTitle()}</b>\n" +
                    $"💰 به مبلغ <b>{order.TotalAmount.ToIranCurrency().En2Fa()} تومان</b>\n\n" +
                    $"<b>{transaction.CreatedOn.ConvertToPersianCalendar().En2Fa()} ساعت {transaction.CreatedOn.ToString("HH:mm").En2Fa()}</b>\n" +
                    $"",
                    ParseMode.Html);
            }

            // if (order.DiscountNumber.HasValue())
            // {
            //     var discount = await _uw.DiscountRepository.GetByDiscountNumberAsync(order.DiscountNumber);
            //     if (discount is not null)
            //     {
            //         var off = discount.Type == DiscountType.Amount
            //             ? discount.Amount
            //             : order.TotalAmount / 100 * discount.Amount;
            //
            //         var code = Transaction.GenerateNewDiscountNumber();
            //         var transaction = new Transaction()
            //         {
            //             TransactionCode = code,
            //             UserId = order.UserId,
            //             IsRemoved = false,
            //             Type = TransactionType.,
            //             CreatedOn = DateTime.Now,
            //             Amount = off
            //         };
            //         _uw.TransactionRepository.Add(transaction);
            //     }
            // }

            var subscriber = await _uw.SubscriberRepository.GetByChatId(order.UserId);
            if (!subscriber.Role.Equals(Role.Colleague) && subscriber.Referral.HasValue())
            {
                var referral = await _uw.SubscriberRepository.GetByChatId(long.Parse(subscriber.Referral));
                if (referral is not null)
                    if (!referral.Role.Equals(Role.Colleague) && referral.IsActive)
                    {
                        var code = Transaction.GenerateNewDiscountNumber();
                        var transaction = new Transaction()
                        {
                            Amount = order.TotalAmount / 100 * 10,
                            CreatedOn = DateTime.Now,
                            TransactionCode = code,
                            UserId = referral.UserId,
                            Type = TransactionType.OrderReward
                        };
                        _uw.TransactionRepository.Add(transaction);
                        await _bot.SendTextMessageAsync(referral.UserId,
                            $".\n" +
                            $"👥 #پاداش_زیرمجموعه به شما تعلق گرفت.\n\n" +
                            $"🔖 شناسه پاداش : <code>#{transaction.TransactionCode}</code>\n" +
                            $"👤 شناسه زیرمجموعه : <code>#U{order.UserId}</code>\n" +
                            $"💰 به مبلغ <b>{transaction.Amount.ToIranCurrency().En2Fa()} تومان</b>\n\n" +
                            $"<b>{transaction.CreatedOn.ConvertToPersianCalendar().En2Fa()} ساعت {transaction.CreatedOn.ToString("HH:mm").En2Fa()}</b>\n" +
                            $".",
                            ParseMode.Html);
                    }
            }
        }
    }


    public static async Task DeclineOrder(this ITelegramBotClient _bot, IUnitOfWork _uw, Order order, User user,
        long chatId, CallbackQuery callBackQuery)
    {
        order.State = OrderState.Declined;
        _uw.OrderRepository.Update(order);

        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "پرداخت با موفقیت رد شد.✅", true);
        await _bot.UpdateDeclinedReceptMessages(order, user, chatId, callBackQuery);
    }
}