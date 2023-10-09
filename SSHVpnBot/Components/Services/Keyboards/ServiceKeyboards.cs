using SSHVpnBot.Components.Colleagues;
using SSHVpnBot.Components.ServiceCategories;
using Telegram.Bot.Types.ReplyMarkups;

namespace SSHVpnBot.Components.Services.Keyboards;

public class ServiceKeyboards
{
    public static IReplyMarkup UpdateServiceCategory(Service service, IEnumerable<ServiceCategory> categories,
        long messageId)
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();
        foreach (var category in categories)
            buttonLines.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData($"🧩 {category.Title}",
                    $"{Constants.ServiceConstants}-updatecategory*{service.Code}*{category.Id}*{messageId}")
            });
        return new InlineKeyboardMarkup(buttonLines);
    }
    // public static InlineKeyboardMarkup ServicesCategoryMangement(IEnumerable<ServiceCategory> categories)
    // {
    //     var buttonLines = new List<List<InlineKeyboardButton>>();
    //     foreach (var item in categories)
    //         buttonLines.Add(new List<InlineKeyboardButton>()
    //         {
    //             InlineKeyboardButton.WithCallbackData(
    //                 $"🔗 {item.Title}",
    //                 $"{Constants.ServiceConstants}-category*{item.Code}")
    //         });
    //
    //     buttonLines.Add(new List<InlineKeyboardButton>()
    //     {
    //         InlineKeyboardButton.WithCallbackData($"افزودن دسته بندی جدید ➕", $"{Constants.CategoryConstants}-newcategory")
    //     });
    //     return new InlineKeyboardMarkup(buttonLines);
    // }

    // public static IReplyMarkup ServiceCategories(Service service, List<ServiceCategory> categories, int message_id)
    // {
    //     var buttonLines = new List<List<InlineKeyboardButton>>();
    //     foreach (var item in categories)
    //         buttonLines.Add(new List<InlineKeyboardButton>()
    //         {
    //             InlineKeyboardButton.WithCallbackData(
    //                 $"{(item.IsActive ? "🟢" : "🔴")} {item.Title}",
    //                 $"servicecategory*{service.Code}*{item.Code}*{message_id}")
    //         });
    //     return new InlineKeyboardMarkup(buttonLines);
    // }

    public static InlineKeyboardMarkup ServiceManagement(IEnumerable<Service> services)
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();
        foreach (var item in services)
            buttonLines.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData(
                    $"🔸 {item.GetFullTitle()}",
                    $"{Constants.ServiceConstants}-service*{item.Code}")
            });

        buttonLines.Add(new List<InlineKeyboardButton>()
        {
            InlineKeyboardButton.WithCallbackData($"افزودن سرویس جدید ➕",
                $"{Constants.ServiceConstants}-newservice")
        });
        buttonLines.Add(new List<InlineKeyboardButton>()
        {
            InlineKeyboardButton.WithCallbackData($"بازگشت 👈", $"{Constants.ServiceConstants}-management")
        });
        return new InlineKeyboardMarkup(buttonLines);
    }

    public static IReplyMarkup NewService(string servicCode)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData("نام 📌️️", $"{Constants.ServiceConstants}-update*{servicCode}*title"),
                InlineKeyboardButton.WithCallbackData("توضیحات ℹ️️️",
                    $"{Constants.ServiceConstants}-update*{servicCode}*description")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("قیمت 💲️️", $"{Constants.ServiceConstants}-update*{servicCode}*price"),
                InlineKeyboardButton.WithCallbackData("مدت ⌛️️️",
                    $"{Constants.ServiceConstants}-update*{servicCode}*duration")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("تعداد کاربر 👥️",
                    $"{Constants.ServiceConstants}-update*{servicCode}*userlimit"),
                InlineKeyboardButton.WithCallbackData("حجم 🔋️️️",
                    $"{Constants.ServiceConstants}-update*{servicCode}*traffic")
            },
            // new()
            // {
            //     InlineKeyboardButton.WithCallbackData("دسته بندی سرویس 🌀",
            //         $"{Constants.ServiceConstants}-update*{servicCode}*category")
            // },
            new()
            {
                InlineKeyboardButton.WithCallbackData("حذف سرویس ✖️",
                    $"{Constants.ServiceConstants}-update*{servicCode}*remove")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("مدیریت قیمت  🤝",
                    $"{Constants.ServiceConstants}-update*{servicCode}*sellerprice")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("افزودن سرویس ✅",
                    $"{Constants.ServiceConstants}-update*{servicCode}*done")
            }
        });
    }
}