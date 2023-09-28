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
        public static IReplyMarkup ColleaguesPriceSettings(OfferRule rule)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData("➕",
                    $"{Constants.ServiceConstants}-collegueprice*{rule.ServiceCode}*less5*plus"),
                InlineKeyboardButton.WithCallbackData($"5 >= X ({rule.LessThan5Order}%)",
                    $"{Constants.ServiceConstants}-collegueprice*{rule.ServiceCode}*none"),
                InlineKeyboardButton.WithCallbackData("➖",
                    $"{Constants.ServiceConstants}-collegueprice*{rule.ServiceCode}*less5*minus")
            },

            new()
            {
                InlineKeyboardButton.WithCallbackData("➕",
                    $"{Constants.ServiceConstants}-collegueprice*{rule.ServiceCode}*more5*plus"),
                InlineKeyboardButton.WithCallbackData($"5 < X ({rule.MoreThan5Order}%)",
                    $"{Constants.ServiceConstants}-collegueprice*{rule.ServiceCode}*none"),
                InlineKeyboardButton.WithCallbackData("➖",
                    $"{Constants.ServiceConstants}-collegueprice*{rule.ServiceCode}*more5*minus")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("➕",
                    $"{Constants.ServiceConstants}-collegueprice*{rule.ServiceCode}*more15*plus"),
                InlineKeyboardButton.WithCallbackData($"15 <= X ({rule.MoreThan15Order}%)٪",
                    $"{Constants.ServiceConstants}-collegueprice*{rule.ServiceCode}*none"),
                InlineKeyboardButton.WithCallbackData("➖",
                    $"{Constants.ServiceConstants}-collegueprice*{rule.ServiceCode}*more15*minus")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("ویرایش قیمت پایه همکاری 🔘",
                    $"{Constants.ServiceConstants}-collegueprice*{rule.ServiceCode}*updatebase")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("بازگشت 👈",
                    $"{Constants.ServiceConstants}-service*{rule.ServiceCode}*back")
            }
        });
    }

    public static InlineKeyboardMarkup ServicesCategoryMangement(IEnumerable<ServiceCategory> categories)
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();
        foreach (var item in categories)
            buttonLines.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData(
                    $"🔗 {item.Title}",
                    $"{Constants.ServiceConstants}-category*{item.Code}")
            });

        buttonLines.Add(new List<InlineKeyboardButton>()
        {
            InlineKeyboardButton.WithCallbackData($"افزودن دسته بندی جدید ➕", $"{Constants.CategoryConstants}-newcategory")
        });
        return new InlineKeyboardMarkup(buttonLines);
    }

    public static IReplyMarkup ServiceCategories(Service service, List<ServiceCategory> categories, int message_id)
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();
        foreach (var item in categories)
            buttonLines.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData(
                    $"{(item.IsActive ? "🟢" : "🔴")} {item.Title}",
                    $"servicecategory*{service.Code}*{item.Code}*{message_id}")
            });
        return new InlineKeyboardMarkup(buttonLines);
    }

    public static InlineKeyboardMarkup ServiceManagement(ServiceCategory category, IEnumerable<Service> services)
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
            InlineKeyboardButton.WithCallbackData($"افزودن سرویس به دسته بندی ➕",
                $"{Constants.ServiceConstants}-newservice*{category.Code}")
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
            new()
            {
                InlineKeyboardButton.WithCallbackData("دسته بندی سرویس 🌀",
                    $"{Constants.ServiceConstants}-update*{servicCode}*category")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("حذف سرویس ✖️",
                    $"{Constants.ServiceConstants}-update*{servicCode}*remove")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("مدیریت قیمت  🤝",
                    $"{Constants.ServiceConstants}-update*{servicCode}*collegues")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("افزودن سرویس ✅",
                    $"{Constants.ServiceConstants}-update*{servicCode}*done")
            }
        });
    }
}