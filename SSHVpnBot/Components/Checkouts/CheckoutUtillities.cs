using ConnectBashBot.Commons;
using ConnectBashBot.Telegram.Handlers;
using SSHVpnBot.Repositories.Uw;
using SSHVpnBot.Telegram;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Components.Checkouts;

public static class CheckoutUtilities
{
    public static async Task DeclineCheckout(this ITelegramBotClient _bot, IUnitOfWork _uw, Checkout checkout,
        User user,
        CallbackQuery callBackQuery)
    {
        var transaction = await _uw.TransactionRepository.GetByCodeAsync(checkout.TransactionCode);
        if (transaction is not null)
            _uw.TransactionRepository.Delete(transaction);

        checkout.State = CheckoutState.Declined;
        _uw.CheckoutRepository.Update(checkout);

        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "درخواست برداشت وجه با موفقیت رد شد.✅", true);
        await _bot.UpdateDeclinedCheckoutMessages(checkout, user, callBackQuery);
    }

    public static async Task UpdateDeclinedCheckoutMessages(this ITelegramBotClient _bot,
        Checkout checkout, User user, CallbackQuery callBackQuery)
    {
        await _bot.SendTextMessageAsync(checkout.UserId,
            $"درخواست برداشت وجه شما با کد پیگیری <b>#{checkout.Code}</b> رد شد ✖️.\n" +
            $"جهت پیگیری علت به پشتیبانی مراجعه نمایید.",
            ParseMode.Html);
        await _bot.EditMessageTextAsync(MainHandler._payments,
            callBackQuery.Message.MessageId,
            callBackQuery.Message.Text.Replace("📌 آیا واریز فوق را انجام دادید؟",
                "" +
                $"توسط {user.FirstName + " " + user.LastName} رد شد✖️"), ParseMode.Html);
    }

    public static async Task ApproveNewCheckout(this ITelegramBotClient _bot, IUnitOfWork _uw, Checkout checkout,
        User user,
        CallbackQuery callBackQuery)
    {
        checkout.State = CheckoutState.Approved;
        _uw.CheckoutRepository.Update(checkout);
        var balance = await _uw.TransactionRepository.GetMineReferralBalance(checkout.UserId);
        await _bot.UpdateApproveCheckoutMessages(checkout, user, balance.Value, callBackQuery);
    }

    public static async Task UpdateApproveCheckoutMessages(this ITelegramBotClient _bot,
        Checkout checkout, User user, decimal balance, CallbackQuery callBackQuery)
    {
        await _bot.Choosed(callBackQuery);
        await _bot.SendTextMessageAsync(checkout.UserId,
            $".\n" +
            $"درخواست برداشت وجه شما با کد پیگیری <b>#{checkout.Code}</b> تایید شد ✅️.\n\n" +
            $"واریز به حساب مورد نظر شما با موفقیت انجام شد.\n" +
            $"💰 به مبلغ <b>{checkout.Amount.ToIranCurrency().En2Fa()} تومان</b>\n" +
            $"💰 موجودی فعلی شما : <b>{balance.ToIranCurrency().En2Fa()} تومان</b>\n" +
            $"🕢 <b>{checkout.CreatedOn.ConvertToPersianCalendar()} ساعت {checkout.CreatedOn.ToString("HH:mm").En2Fa()}</b>\n\n" +
            $".",
            ParseMode.Html);
        await _bot.EditMessageTextAsync(MainHandler._payments,
            callBackQuery.Message.MessageId,
            callBackQuery.Message.Text.Replace("📌 آیا واریز فوق را انجام دادید؟",
                "" +
                $"توسط {user.FirstName + " " + user.LastName} واریز شد.✅️"), ParseMode.Html);
    }
}