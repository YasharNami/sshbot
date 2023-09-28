using ConnectBashBot.Commons;
using ConnectBashBot.Telegram.Handlers;
using SSHVpnBot.Components.Base;
using SSHVpnBot.Components.ServiceCategories.Keyboards;
using SSHVpnBot.Components.Subscribers;
using SSHVpnBot.Repositories.Uw;
using SSHVpnBot.Telegram;
using SSHVpnBot.Telegram.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Components.ServiceCategories.Handlers;

public class ServiceCategoryCallbackHandler : QueryHandler
{
        public ServiceCategoryCallbackHandler(ITelegramBotClient _bot, Update update,
        IUnitOfWork _uw,
        Subscriber subscriber, CancellationToken cancellationToken = default)
        :base(_bot,update,_uw,subscriber)
    {
        
    }
    public override async Task QueryHandlerAsync()
    {
        if (data.Equals("management"))
        {
            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "مدیریت دسته بندی ها انتخاب شد.");
            var categories = await _uw.ServiceCategoryRepository.GetAllCategoriesAsync();
            await _bot
                .SendTextMessageAsync(groupId,
                    $"مدیریت دسته بندی ها :\n\n" +
                    $"🟢️ دسته بندی های فعال : <b>{categories.Count(s => s.IsActive).En2Fa()} عدد</b>\n" +
                    $"🔴️️ دسته بندی های غیرفعال : <b>{categories.Count(s => !s.IsActive).En2Fa()} عدد</b>\n\n" +
                    $"قصد مدیریت کدام سرویس را دارید؟",
                    ParseMode.Html,
                    replyMarkup: ServiceCategoryKeyboards.CategoryManagement(categories));
        }
        else if (data.StartsWith("category*"))
        {
            var category = await _uw.ServiceCategoryRepository.GetByServiceCategoryCode(data.Split("*")[1]);
            if (category is not null)
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                    "اطلاعات دسته بندی جهت ویرایش برای شما ارسال شد.", true);
                await _bot.UpdateCategoryMessage(_uw, user.Id, category);
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "دسته بندی مورد نظر یافت نشد.", true);
            }
        }
        else if (data.Equals("newcategory"))
        {
            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                "اطلاعات دسته بندی جدید جهت ویرایش برای شما ارسال شد.", true);

            var category = new ServiceCategory()
            {
                Description = "",
                Title = "",
                IsActive = false,
                Code =ServiceCategory.GenerateNewCode()
            };
            _uw.ServiceCategoryRepository.Add(category);

            await _bot.UpdateCategoryMessage(_uw, user.Id, category);
        }
        else if (data.StartsWith("update*"))
        {
            var category = await _uw.ServiceCategoryRepository.GetByServiceCategoryCode(data.Split("*")[1]);
            if (category is null)
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "دسته بندی یافت نشد.", true);
                return;
            }

            var property = data.Split("*")[2];

            switch (property)
            {
                case "title":
                    await _bot.Choosed(callBackQuery);
                    await _bot.SendTextMessageAsync(user.Id, "🔗 نام دسته بندی را ارسال کنید :",
                        replyMarkup: MarkupKeyboards.Cancel());
                    _uw.SubscriberRepository.ChangeStep(user.Id,
                        $"{Constants.CategoryConstants}-update*{category.Code}*title*{callBackQuery.Message.MessageId}");
                    break;
                case "description":
                    await _bot.Choosed(callBackQuery);
                    await _bot.SendTextMessageAsync(user.Id, "📝 توضیحات دسته بندی را ارسال کنید :",
                        replyMarkup: MarkupKeyboards.Cancel());
                    _uw.SubscriberRepository.ChangeStep(user.Id,
                        $"{Constants.CategoryConstants}-update*{category.Code}*description*{callBackQuery.Message.MessageId}");
                    break;
                case "activation":
                    category.IsActive = !category.IsActive;
                    _uw.ServiceCategoryRepository.Update(category);
                    if (category.IsActive)
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                            "سته بندی با موفقیت غیرفعال شد.", true);
                    else
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                            "دسنه بندی با موفقیت فعال شد.", true);
                    await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);

                    await _bot.UpdateCategoryMessage(_uw, user.Id, category);
                    await _bot.SendTextMessageAsync(MainHandler._managementgroup,
                        $"<b>دسته بندی به شرح زیر توسط {user.FirstName + " " + user.LastName} {(category.IsActive ? "قعال" : "غیرفعال")} شد.✔️</b>\n\n" +
                        $"🔖 شناسه : <code>#{category.Code}</code>\n" +
                        $"📌 نام دسته بندی : <b>{category.Title}</b>\n",
                        ParseMode.Html
                    );
                    break;
                case "delete":
                    category.IsRemoved = true;
                    _uw.ServiceCategoryRepository.Update(category);
                    await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "دسته بندی با موفقیت حذف شد.", true);

                    await _bot.SendTextMessageAsync(MainHandler._managementgroup,
                        $"<b>دسته بندی به شرح زیر توسط {user.FirstName + " " + user.LastName} جذف شد.✔️</b>\n\n" +
                        $"🔖 شناسه : <code>#{category.Code}</code>\n" +
                        $"📌 نام دسته بندی : <b>{category.Title}</b>\n",
                        ParseMode.Html
                    );
                    break;
                case "done":
                    if (category.Title.HasValue())
                    {
                        category.IsActive = true;
                        _uw.ServiceCategoryRepository.Update(category);
                        await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
                        await _bot.SendTextMessageAsync(MainHandler._managementgroup,
                            $"<b>دسته بندی به شرح زیر توسط {user.FirstName + " " + user.LastName} ویرایش/اضافه شد.✔️</b>\n\n" +
                            $"🔖 شناسه : <code>#{category.Code}</code>\n" +
                            $"📌 نام دسته بندی : <b>{category.Title}</b>\n",
                            ParseMode.Html
                        );
                    }
                    else
                    {
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                            "پیش از ویرایش دسته بندی اطلاعات مورد نیاز را تکمیل نمایید.", true);
                    }

                    break;
            }
        }
    }
}