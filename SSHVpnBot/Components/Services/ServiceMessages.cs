using ConnectBashBot.Commons;
using ConnectBashBot.Telegram.Handlers;
using SSHVpnBot.Components.Colleagues;
using SSHVpnBot.Components.ServiceCategories;
using SSHVpnBot.Components.Services.Keyboards;
using SSHVpnBot.Repositories.Uw;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Components.Services;

public static class ServiceMessages
{
    public static async Task UpdateService(this ITelegramBotClient bot, IUnitOfWork uw, long chatId, Service service)
    {
        // ServiceCategory category = null;
        // if (service.CategoryCode.HasValue())
        //     category = await uw.ServiceCategoryRepository.GetByServiceCategoryCode(service.CategoryCode);
        await bot
            .SendTextMessageAsync(chatId,
                $"<b>ویرایش سرویس <code>#{service.Code}</code> :</b>\n\n" +
                $"📌 نام سرویس : <b>{service.GetFullTitle()}</b>\n" +
                //(category is null ? "" : $"🧩 دسته بندی سرویس : <b>{category.Title}</b>\n") +
                $"💬️ توضیحات : <b>{(service.Description.HasValue() ? service.Description : "تنظیم نشده")}</b>\n" +
                $"💲 قیمت سرویس : {service.Price.ToIranCurrency().En2Fa()} تومان\n" +
                $"💲 قیمت پایه همکار : {(service.SellerPrice).ToIranCurrency().En2Fa()} تومان\n" +
                $"⌛️ مدت : <b>{(service.Duration == 0 ? "نامحدود" : service.Duration.ToString().En2Fa() + " روز")}</b>\n" +
                $"♻️ جهت ویرایش از منو زیر اقدام نمایید :",
                ParseMode.Html,
                replyMarkup: ServiceKeyboards.NewService(service.Code));
    }

    public static async Task AddNewService(this ITelegramBotClient bot, IUnitOfWork uw, long chatId, Service service)
    {
        ServiceCategory category = null;
        // if (service.CategoryCode.HasValue())
        //     category = await uw.ServiceCategoryRepository.GetByServiceCategoryCode(service.CategoryCode);
        await bot
            .SendTextMessageAsync(chatId,
                $"<b>افزودن سرویس جدید :</b>\n\n" +
                $"🔖 شناسه سرویس : <code>#{service.Code}</code>\n" +
                $"📌 نام سرویس : <b>{service.GetFullTitle()}</b>\n" +
                //(category is null ? "" : $"🧩 دسته بندی سرویس : <b>{category.Title}</b>\n") +
                $"💬️ توضیحات : <b>{(service.Description.HasValue() ? service.Description : "تنظیم نشده")}</b>\n" +
                $"💲 قیمت سرویس : {service.Price.ToIranCurrency().En2Fa()} تومان\n" +
                $"⌛️ مدت : <b>{(service.Duration == 0 ? "نامحدود" : service.Duration.ToString().En2Fa() + " روز")}</b>\n\n" +
                $"♻️ جهت ویرایش از منو زیر اقدام نمایید :",
                ParseMode.Html,
                replyMarkup: ServiceKeyboards.NewService(service.Code));
    }

    public static async void NewServiceAddReport(this ITelegramBotClient bot, long chatId, Service service, User user)
    {
        await bot.SendTextMessageAsync(MainHandler._managementgroup,
            $"<b>سرویس جدید به شرح زیر توسط {user.FirstName + " " + user.LastName} ویرایش/اضافه شد.✔️</b>\n\n" +
            $"🔖 شناسه سرویس : <code>#{service.Code}</code>\n" +
            $"📌 نام سرویس : <b>{service.GetFullTitle()}</b>\n" +
            $"💬️ توضیحات : <b>{(service.Description.HasValue() ? service.Description : "تنظیم نشده")}</b>\n" +
            $"💲 قیمت سرویس : {service.Price.ToIranCurrency().En2Fa()} تومان",
            ParseMode.Html
        );
    }

    public static async void ServiceRemovedReport(this ITelegramBotClient bot, long chatId, Service service, User user)
    {
        await bot.SendTextMessageAsync(MainHandler._managementgroup,
            $"<b>سرویس جدید به شرح زیر توسط {user.FirstName + " " + user.LastName} حذف شد.✔️</b>\n\n" +
            $"🔖 شناسه سرویس : <code>#{service.Code}</code>\n" +
            $"📌 نام سرویس : <b>{service.GetFullTitle()}</b>\n" +
            $"💬️ توضیحات : <b>{(service.Description.HasValue() ? service.Description : "تنظیم نشده")}</b>\n" +
            $"💲 قیمت سرویس : {service.Price.ToIranCurrency().En2Fa()} تومان",
            ParseMode.Html
        );
    }
}