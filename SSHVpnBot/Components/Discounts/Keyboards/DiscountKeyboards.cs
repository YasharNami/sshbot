using ConnectBashBot.Commons;
using SSHVpnBot.Components.Services;
using Telegram.Bot.Types.ReplyMarkups;

namespace SSHVpnBot.Components.Discounts.Keyboards;

public class DiscountKeyboards
{
    public static IReplyMarkup DiscountManagement(IEnumerable<Discount> discounts)
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();
        foreach (var item in discounts)
        {
            var status = item.IsActive ? "(فعال ✅)" : "(غیرفعال ☑️)";
            buttonLines.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData(
                    $"🔖 {item.UsedCount.ToString().En2Fa()} - {item.Code} {status}",
                    $"{Constants.DiscountConstants}-discount*{item.DiscountNumber}")
            });
        }

        buttonLines.Add(new List<InlineKeyboardButton>()
        {
            InlineKeyboardButton.WithCallbackData($"افزودن کد تخفیف ➕", $"{Constants.DiscountConstants}-newdiscount")
        });
        return new InlineKeyboardMarkup(buttonLines);
    }

    public static IReplyMarkup DiscountServices(Discount discount, List<Service> services, int message_id)
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();
        foreach (var item in services)
            buttonLines.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData($"🧩 {item.GetFullTitle()}",
                    $"{Constants.DiscountConstants}-service*{discount.Code}*{item.Code}*{message_id}")
            });

        return new InlineKeyboardMarkup(buttonLines);
    }

    public static IReplyMarkup NewDiscountCode(Discount discount)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData("کد تخفیف 🔖️",
                    $"{Constants.DiscountConstants}-update*{discount.Id}*code"),
                InlineKeyboardButton.WithCallbackData("نوع 🎁️️️", $"{Constants.DiscountConstants}-update*{discount.Id}*type")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("مقدار تخفیف 💲",
                    $"{Constants.DiscountConstants}-update*{discount.Id}*amount")
            },
            // new()
            // {
            //     InlineKeyboardButton.WithCallbackData("تعیین سقف میزان درصد 📍",
            //         $"{Constants.DiscountConstants}-update*{discount.Id}*maxpercentamount")
            // },
            // new()
            // {
            //     InlineKeyboardButton.WithCallbackData("اختصاص به سرویس خاص 🧩",
            //         $"{Constants.DiscountConstants}-update*{discount.Id}*service")
            // },
            // new()
            // {
            //     InlineKeyboardButton.WithCallbackData("اختصاص به کاربر خاص 👤",
            //         $"{Constants.DiscountConstants}-update*{discount.Id}*service")
            // },
            new()
            {
                InlineKeyboardButton.WithCallbackData("تعداد استفاده 👥️",
                    $"{Constants.DiscountConstants}-update*{discount.Id}*usage"),
                InlineKeyboardButton.WithCallbackData("تاریخ انقضا ⌛️️️️",
                    $"{Constants.DiscountConstants}-update*{discount.Id}*expiredon")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("حذف کد تخفیف ✖️",
                    $"{Constants.DiscountConstants}-update*{discount.Id}*remove")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("افزودن کد تخفیف ✅",
                    $"{Constants.DiscountConstants}-update*{discount.Id}*done")
            }
        });
    }

    public static IReplyMarkup DiscountTypes(Discount discount, int messageId)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData("🔻 درصدی 🔻",
                    $"{Constants.DiscountConstants}-type*{discount.Id}*percent*{messageId}"),
                InlineKeyboardButton.WithCallbackData("🔻 مقداری 🔻️️️",
                    $"{Constants.DiscountConstants}-type*{discount.Id}*amount*{messageId}")
            }
        });
    }

    public static IReplyMarkup RemoveDiscountConfirmation(Discount discount)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData($"بله اطمینان دارم 👍",
                    $"{Constants.DiscountConstants}-remove*approve*{discount.Id}")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"خیر مطمین نیستم 👍",
                    $"{Constants.DiscountConstants}-remove*decline*{discount.Id}")
            }
        });
    }

    public static IReplyMarkup DiscountDurations(Discount discount, int messageId)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData("⌛️ ۲۴ ساعت آینده",
                    $"{Constants.DiscountConstants}-duration*{discount.Id}*1*{messageId}"),
                InlineKeyboardButton.WithCallbackData("⌛ یک ماه آینده",
                    $"{Constants.DiscountConstants}-duration*{discount.Id}*30*{messageId}")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("⌛ سه ماه آینده",
                    $"{Constants.DiscountConstants}-duration*{discount.Id}*90*{messageId}")
            }
        });
    }
}