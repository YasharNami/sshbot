using ConnectBashBot.Commons;
using ConnectBashBot.Telegram.Handlers;
using SSHVpnBot.Repositories.Uw;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Components.Payments;

public static class PaymentUtillities
{
    public static async Task DeclinePayment(this ITelegramBotClient _bot, IUnitOfWork _uw, Payment payment, User user,
        long chatId, CallbackQuery callBackQuery)
    {
        payment.State = PaymentState.Declined;
        _uw.PaymentRepository.Update(payment);

        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "فیش با موفقیت رد شد.✅", true);
        await _bot.UpdateDeclinedPaymentReceptMessages(payment, user, chatId, callBackQuery);
    }

    public static async Task UpdateDeclinedPaymentReceptMessages(this ITelegramBotClient _bot,
        Payment payment, User user,
        long chatId, CallbackQuery callBackQuery)
    {
        await _bot.SendTextMessageAsync(payment.UserId,
            $"رسید درخواست شارژ شما با کد پیگیری <b>#{payment.PaymentCode}</b> رد شد ✖️.\n" +
            $"جهت پیگیری علت به پشتیبانی مراجعه نمایید.",
            ParseMode.Html);
        await _bot.EditMessageTextAsync(MainHandler._payments,
            callBackQuery.Message.MessageId,
            callBackQuery.Message.Text.Replace("♻️ آیا اطلاعات فوق را جهت شارژ حساب تایید میکنید؟",
                "" +
                $"توسط {user.FirstName + " " + user.LastName} رد شد✖️"));
    }

    public static async Task UpdateApprovedPaymentReceptMessages(this ITelegramBotClient _bot,
        Payment payment, User user,
        long chatId, decimal balance, CallbackQuery callBackQuery)
    {
        await _bot.SendTextMessageAsync(payment.UserId,
            $".\n" +
            $"رسید سفارش شما با کد پیگیری <b>#{payment.PaymentCode}</b> تایید شد ✅️.\n\n" +
            $"شارژ حساب کاربری شما با موفقیت انجام شد.\n" +
            $"💰 به مبلغ <b>{payment.Amount.ToIranCurrency().En2Fa()} تومان</b>\n" +
            $"💰 موجودی فعلی شما : <b>{balance.ToIranCurrency().En2Fa()} تومان</b>\n" +
            $"🕢 <b>{payment.CreatedOn.ConvertToPersianCalendar()} ساعت {payment.CreatedOn.ToString("HH:mm").En2Fa()}</b>\n\n" +
            $".",
            ParseMode.Html);
        await _bot.EditMessageTextAsync(MainHandler._payments,
            callBackQuery.Message.MessageId,
            callBackQuery.Message.Text.Replace("♻️ آیا اطلاعات فوق را جهت شارژ حساب تایید میکنید؟",
                "" +
                $"توسط {user.FirstName + " " + user.LastName} تایید شد.✅️"));
    }
}