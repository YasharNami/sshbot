using ConnectBashBot.Commons;
using ConnectBashBot.Telegram.Handlers;
using SSHVpnBot.Components.Accounts;
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