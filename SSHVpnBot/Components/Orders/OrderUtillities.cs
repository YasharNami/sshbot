using ConnectBashBot.Commons;
using ConnectBashBot.Telegram.Handlers;
using SSHVpnBot.Components.Accounts;
using SSHVpnBot.Components.Discounts;
using SSHVpnBot.Components.Servers;
using SSHVpnBot.Components.Services;
using SSHVpnBot.Components.Subscribers;
using SSHVpnBot.Components.Transactions;
using SSHVpnBot.Repositories.Uw;
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

        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "پرداخت با موفقیت تایید شد.✅", true);

        var account = await _uw.AccountRepository.GetByAccountCode(order.AccountCode);
        var server = await _uw.ServerRepository.GetServerByCode(account.ServerCode);
        
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
            await _bot.CreateSingleClientOnServer(_uw, server, service, order, user, chatId,callBackQuery);

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
                   if (!order.DiscountNumber.HasValue())
                                {
                                    var cashbackcode = Transaction.GenerateNewDiscountNumber();
                                    var trans = new Transaction()
                                    {
                                        Amount = (order.TotalAmount / 100 ) * 5,
                                        CreatedOn = DateTime.Now,
                                        UserId = order.UserId,
                                        TransactionCode = cashbackcode,
                                        IsRemoved = false,
                                        Type = TransactionType.CashBack
                                    };
                                    _uw.TransactionRepository.Add(trans);
                                }
                   var balance = await _uw.TransactionRepository.GetMineBalanceAsync(order.UserId);
                   Discount discount = null;
                   if (order.DiscountNumber.HasValue())
                   {
                       discount = await _uw.DiscountRepository.GetByDiscountNumberAsync(order.DiscountNumber);
                   }
                   await _bot.SendTextMessageAsync(MainHandler._payments,
                       $".\n" +
                       $"<b>اطلاعات سفارش ثبت شده 🟢️</b>\n\n" +
                       $"🔖 <b>#{order.TrackingCode}</b>\n" +
                       $"🔗 <b>{order.Count.En2Fa()} اشتراک {service.GetFullTitle()}</b>\n" +
                       $"💳 <b>{order.TotalAmount.ToIranCurrency().En2Fa()} تومان</b>\n" +
                       $"📅 <b>{order.CreatedOn.ConvertToPersianCalendar().En2Fa()} ساعت {order.CreatedOn.ToString("HH:mm").En2Fa()}</b>\n" +
                       $"👤 <code>#U{chatId}</code> | <a href='tg://user?id={chatId}'>{user.FirstName} {user.LastName}</a>\n" +
                       $"{(discount is null ? "" : $"🔖 کد تخفیف : <b>{discount.Code}</b>\n")}\n" +
                       $"💰 موجودی پس از سفارش : <b>{balance.Value.ToIranCurrency().En2Fa()} تومان</b>\n\n" +
                       $"تایید شده توسط سیستم ✅️",
                       ParseMode.Html);
            }

            if (order.DiscountNumber.HasValue())
            {
                var discount = await _uw.DiscountRepository.GetByDiscountNumberAsync(order.DiscountNumber);
                if (discount is not null)
                {
                    var off = discount.Type == DiscountType.Amount
                        ? discount.Amount
                        : order.TotalAmount / 100 * discount.Amount;

                    var code = Transaction.GenerateNewDiscountNumber();
                    var transaction = new Transaction()
                    {
                        TransactionCode = code,
                        UserId = order.UserId,
                        IsRemoved = false,
                        Type = TransactionType.CashBack,
                        CreatedOn = DateTime.Now,
                        Amount = off
                    };
                    _uw.TransactionRepository.Add(transaction);
                }
            }
           
            var subscriber =  await _uw.SubscriberRepository.GetByChatId(order.UserId);
            if (!subscriber.Role.Equals(Role.Colleague) && subscriber.Referral.HasValue())
            {
                var referral = await _uw.SubscriberRepository.GetByChatId(long.Parse(subscriber.Referral));
                if (referral is not null)
                    if (!referral.Role.Equals(Role.Colleague) && referral.IsActive)
                    {
                        var code = Transaction.GenerateNewDiscountNumber();
                        var transaction = new Transaction()
                        {
                            Amount = (order.TotalAmount / 100) * 10,
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