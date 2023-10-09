using ConnectBashBot.Commons;
using SSHVpnBot.Components.Locations;
using SSHVpnBot.Components.Servers.Keyboards;
using SSHVpnBot.Components.ServiceCategories;
using SSHVpnBot.Repositories.Uw;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Components.Servers;

public static class ServerMessages
{
    public static async Task AddNewServer(this ITelegramBotClient bot, IUnitOfWork uw, long chatId, Server server)
    {
        // ServiceCategory? category = null;
        // if (server.CategoryCode.HasValue())
        //     category = await uw.ServiceCategoryRepository.GetByServiceCategoryCode(server.CategoryCode);

        Location location = null;
        if (server.LocationCode.HasValue())
            location = await uw.LocationRepository.GetLocationByCode(server.LocationCode);
        await bot
            .SendTextMessageAsync(chatId,
                $"<b>افزودن سرور جدید :</b>\n\n" +
                $"🔖 شناسه سرور : <code>#{server.Code}</code>\n" +
                $"🔗 آدرس سرور : <code>{server.Url}</code>\n" +
                $"🔘 نوع سرور : <code>{server.Type.ToDisplay()}</code>\n" +
                $"📍 آدرس دامنه : <code>{server.Domain}</code>\n" +
                $"🔘 نام کاربری : <code>{server.Username}</code>\n\n" +
                $"{(location is not null ? $"🌍 لوکیشن سرور : <b>{location.Title} {location.Flat}</b>\n" : "")}" +
                //$"{(category is not null ? $"🌀 دسته بندی : <b>{category.Title}</b>\n\n" : "")}" +
                $"♻️ جهت ویرایش از منو زیر اقدام نمایید :",
                ParseMode.Html,
                replyMarkup: ServerKeyboards.SingleServerManagement(server));
    }
}