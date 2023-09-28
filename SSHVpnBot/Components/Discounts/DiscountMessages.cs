using ConnectBashBot.Commons;
using ConnectBashBot.Telegram.Handlers;
using SSHVpnBot.Components.Discounts.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Components.Discounts;
public static class DiscountMessages
{
    public static async Task AddNewDiscount(this ITelegramBotClient bot, long chatId, Discount discount)
    {
        await bot
            .SendTextMessageAsync(chatId,
                $"<b>افزودن کد تخفیف جدید :</b>\n\n" +
                $"🔖 شناسه کدتخفیف : <code>#{discount.DiscountNumber}</code>\n" +
                $"📌 کد : <b>{discount.Code}</b>\n" +
                $"💬️ نوع کد تخفیف : <b>{discount.Type.ToDisplay()}</b>\n" +
                $"💲 مقدار تخفیف : <b>{(discount.Type == DiscountType.Amount ? discount.Amount.ToIranCurrency().En2Fa() + " تومان" : discount.Amount.En2Fa() + " درصد")}</b>\n" +
                $"⌛ تاریخ انقضا : <b>{discount.ExpiredOn.ConvertToPersianCalendar()}</b>\n" +
                $"👥 تعداد استفاده : <b>{(discount.UsageLimitation == 0 ? "بدون محدودیت" : discount.UsageLimitation.ToString().En2Fa() + " کاربر")}</b>\n" +
                $"📍 وضعیت : <b>{(discount.IsActive ? "فعال 🟢" : "غیرفعال 🔴")}</b>\n\n" +
                $"♻️ جهت ویرایش از منو زیر اقدام نمایید :",
                ParseMode.Html,
                replyMarkup: DiscountKeyboards.NewDiscountCode(discount));
    }

    public static async void DiscountRemovedReport(this ITelegramBotClient bot, long chatId, Discount discount,
        User user)
    {
        await bot.SendTextMessageAsync(MainHandler._managementgroup,
            $"<b>کد تخفیف به شرح زیر توسط {user.FirstName + " " + user.LastName} حذف شد.✔️</b>\n\n" +
            $"🔖 شناسه کدتخفیف : <code>#{discount.DiscountNumber}</code>\n" +
            $"📌 کد : <b>{discount.Code}</b>\n" +
            $"💬️ نوع کدتخفیف : <b>{discount.Type.ToDisplay()}</b>\n" +
            $"💲 مقدار تخفیف : <b>{(discount.Type == DiscountType.Amount ? discount.Amount.ToIranCurrency().En2Fa() + " تومان" : discount.Amount.En2Fa() + " درصد")}</b>\n" +
            $"⌛ تاریخ انقضا : <b>{(discount.ExpiredOn == default ? "بدون محدودیت" : discount.ExpiredOn.ToPersianDate().En2Fa())}</b>\n" +
            $"👥 تعداد استفاده : <b>{(discount.UsageLimitation == 0 ? "بدون محدودیت" : discount.UsageLimitation.ToString().En2Fa() + " کاربر")}</b>\n" +
            $"📍 وضعیت : <b>{(discount.IsActive ? "فعال 🟢" : "غیرفعال 🔴")}</b>\n\n",
            ParseMode.Html
        );
    }
}