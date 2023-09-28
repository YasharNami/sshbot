using Telegram.Bot.Types.ReplyMarkups;

namespace SSHVpnBot.Components.ServiceCategories.Keyboards;


public class ServiceCategoryKeyboards
{
    public static InlineKeyboardMarkup CategoryManagement(IEnumerable<ServiceCategory> categories)
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();
        foreach (var item in categories)
            buttonLines.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData(
                    $"{(item.IsActive ? "🟢" : "🔴")} {item.Title}",
                    $"{Constants.CategoryConstants}-category*{item.Code}")
            });

        buttonLines.Add(new List<InlineKeyboardButton>()
        {
            InlineKeyboardButton.WithCallbackData($"افزودن دسته بندی جدید ➕", $"{Constants.CategoryConstants}-newcategory")
        });
        return new InlineKeyboardMarkup(buttonLines);
    }

    public static IReplyMarkup SingleCategoryManagement(ServiceCategory category)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData("نام 📌️️️️",
                    $"{Constants.CategoryConstants}-update*{category.Code}*title"),
                InlineKeyboardButton.WithCallbackData("توضیحات 📝️",
                    $"{Constants.CategoryConstants}-update*{category.Code}*description")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("حذف دسته بندی ✖",
                    $"{Constants.CategoryConstants}-update*{category.Code}*delete")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData(
                    $"{(category.IsActive ? "غیرفعال سازی 🔴️️️" : "فعال سازی 🟢️️️️")}",
                    $"{Constants.CategoryConstants}-update*{category.Code}*activation")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("ویرایش دسته بندی ➕",
                    $"{Constants.CategoryConstants}-update*{category.Code}*done")
            }
        });
    }
}