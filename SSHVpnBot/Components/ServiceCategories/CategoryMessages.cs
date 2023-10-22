using ConnectBashBot.Commons;
using SSHVpnBot.Components.ServiceCategories.Keyboards;
using SSHVpnBot.Repositories.Uw;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Components.ServiceCategories;

public static class CategoryMessages
{
    public static async Task UpdateCategoryMessage(this ITelegramBotClient bot, IUnitOfWork uw, long chatId,
        ServiceCategory category)
    {
        var services = await uw.ServiceRepository.GetServicesByCategoryCodeAsync(category.Code);
        
        await bot.SendTextMessageAsync(chatId,
            $".\n" +
            $"🌀 اطلاعات دسته بندی :\n\n" +
            $"🔖 شناسه : <code>#{category.Code}</code>\n" +
            $"📌 نام : <b>{category.Title}</b>\n" +
            $"📝 توضیحات : \n" +
            $"{category.Description}\n" +
            $"📍 وضعیت : <b>{(category.IsActive ? "فعال 🟢" : "غیرفعال 🔴")}</b>\n\n" +
            $"🧩 تعداد سرویس های این دسته بندی : <b>{services.Count.En2Fa()}</b>\n" +
            $"قصد ویرایش کدام آیتم را دارید؟",
            ParseMode.Html,
            replyMarkup: ServiceCategoryKeyboards.SingleCategoryManagement(category));
    }
}