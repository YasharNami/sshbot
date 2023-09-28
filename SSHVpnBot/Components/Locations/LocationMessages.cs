using SSHVpnBot.Components.Locations.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Components.Locations;

public static class LocationMessages
{
    public static async Task AddNewLocation(this ITelegramBotClient bot, long chatId, Location location)
    {
        await bot.SendTextMessageAsync(chatId,
            $"<b>اطلاعات موقعیت مکانی <code>#{location.Code}</code></b>\n\n" +
            $"📌 نام موقعیت مکانی :\n" +
            $"<b>{location.Title}</b>\n" +
            $"🏳️ پرچم : {location.Flat}\n",
            ParseMode.Html,
            replyMarkup: LocationKeyboards.SingleLocationManagement(location));
    }
}