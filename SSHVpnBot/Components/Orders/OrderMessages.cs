using ConnectBashBot.Commons;
using SSHVpnBot.Components.Orders.Keyboards;
using SSHVpnBot.Components.PaymentMethods;
using SSHVpnBot.Components.Services;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Components.Orders;

public static class OrderMessages
{
    public static async Task ReviewOrder(this ITelegramBotClient _bot, long chatId, IEnumerable<PaymentMethod> payments,
        Service service, Order order)
    {
        var cashback = order.TotalAmount / 100 * 5;
        await _bot.SendTextMessageAsync(chatId,
            $".\n" +
            $"<b>♻️ بررسی و تایید سفارش</b>\n\n" +
            $"🔗 اشتراک : <b>{service.GetFullTitle()}</b>\n" +
            $"💰 قیمت فاکتور : <b>{order.TotalAmount.ToIranCurrency().En2Fa()} تومان</b>\n\n" +
            $"🌀 در صورت پرداخت از کیف پول مبلغ <b>{cashback.ToIranCurrency().En2Fa()} تومان</b>\n" +
            $" به کیف پول شما باز می گردد.\n\n" +
            $"▫️نحوه‌ی پرداخت خود را انتخاب کنید :".En2Fa(),
            ParseMode.Html,
            replyMarkup: OrderKeyboards.PaymentMethods(payments, service, order.TrackingCode));
    }

    public static async Task ReviewOrderAfterDiscount(this ITelegramBotClient _bot,
        long chatId, IEnumerable<PaymentMethod> payments,
        Service service
        , Order order)
    {
        await _bot.SendTextMessageAsync(chatId,
            $".\n" +
            $"<b>♻️ بررسی و تایید سفارش</b>\n\n" +
            $"🔗 اشتراک : <b>{service.GetFullTitle()}</b>\n" +
            $"💰 قیمت فاکتور :<b>{order.TotalAmount.ToIranCurrency().En2Fa()} تومان</b>\n\n" +
            $"▫️نحوه‌ی پرداخت خود را انتخاب کنید:".En2Fa(),
            ParseMode.Html,
            replyMarkup: OrderKeyboards.PaymentMethodsAfterDiscount(payments, service, order.TrackingCode));
    }
}