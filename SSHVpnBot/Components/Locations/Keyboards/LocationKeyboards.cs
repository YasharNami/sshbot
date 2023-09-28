using Telegram.Bot.Types.ReplyMarkups;

namespace SSHVpnBot.Components.Locations.Keyboards;

public class LocationKeyboards
{
    public static IReplyMarkup LocationManagement(IEnumerable<Location> locations)
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();

        foreach (var location in locations)
            buttonLines.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData(
                    $"{location.Title} ({location.Flat})",
                    $"location*{location.Code}")
            });
        buttonLines.Add(new List<InlineKeyboardButton>()
        {
            InlineKeyboardButton.WithCallbackData($"افزودن موقعیت مکانی ➕", $"add-new-location")
        });
        return new InlineKeyboardMarkup(buttonLines);
    }

    public static IReplyMarkup SingleLocationManagement(Location location)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData("نام موقعیت مکانی 📌", $"updatelocation*{location.Code}*title")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("پرچم کشور 🏳️", $"updatelocation*{location.Code}*flat")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("افزودن موقعیت مکانی ✅", $"updatelocation*{location.Code}*done")
            }
        });
    }
}