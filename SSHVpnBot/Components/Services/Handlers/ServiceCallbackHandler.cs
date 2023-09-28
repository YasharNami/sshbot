using ConnectBashBot.Commons;
using SSHVpnBot.Components.Base;
using SSHVpnBot.Components.Colleagues;
using SSHVpnBot.Components.Services.Keyboards;
using SSHVpnBot.Components.Subscribers;
using SSHVpnBot.Repositories.Uw;
using SSHVpnBot.Telegram;
using SSHVpnBot.Telegram.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Components.Services.Handlers;

public class ServiceCallbackHandler : QueryHandler
{
    public ServiceCallbackHandler(ITelegramBotClient _bot, Update update,
        IUnitOfWork _uw,
        Subscriber subscriber, CancellationToken cancellationToken = default)
        :base(_bot,update,_uw,subscriber)
    {
        
    }
    public override async Task QueryHandlerAsync()
    {
        if (data.Equals("management"))
        {
            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "مدیریت سرویس ها انتخاب شد.");

            var categories = await _uw.ServiceCategoryRepository.GetAllCategoriesAsync();
            var services = _uw.ServiceRepository.GetAll().Where(s => !s.IsRemoved).ToList();
            await _bot.DeleteMessageAsync(groupId, callBackQuery.Message.MessageId);
            await _bot
                .SendTextMessageAsync(groupId,
                    $"مدیریت سرویس ها :\n\n" +
                    $"✔️ تعداد سرویس های فعال : <b>{services.Where(s => s.IsActive).ToList().Count} عدد</b>\n" +
                    $"✖️️ تعداد سرویس های غیرفعال : <b>{services.Where(s => !s.IsActive).ToList().Count} عدد</b>\n\n" +
                    $"قصد مدیریت کدام نوع از سرویس ها را دارید؟",
                    ParseMode.Html,
                    replyMarkup: ServiceKeyboards.ServicesCategoryMangement(categories));
        }
        else if (data.StartsWith("service*"))
        {
            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                "اطلاعات سرویس مورد نظر جهت ویرایش برای شما ارسال شد.✔️", true);

            var serviceCode = data.Split("*")[1];
            var service = await _uw.ServiceRepository.GetServiceByCode(serviceCode);
            await _bot.AddNewService(_uw, user.Id, service);
        }
        else if (data.StartsWith("newservice*"))
        {
            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                "اطلاعات سرویس جدید جهت ویرایش برای شما ارسال شد.✔️", true);

            var servicCode = Service.GenerateNewCode();
            var serivce = new Service()
            {
                CategoryCode = data.Split("*")[1],
                Code = servicCode
            };
            await _bot
                .SendTextMessageAsync(user.Id,
                    $"<b>افزودن سرویس جدید :</b>\n\n" +
                    $"🔖 شناسه سرویس : <code>#{servicCode}</code>\n" +
                    $"📌 نام سرویس : <b>{serivce.Title}</b>\n" +
                    $"💬️ توضیحات : <b>{(serivce.Description.HasValue() ? serivce.Description : "تنظیم نشده")}</b>\n" +
                    $"💲 قیمت سرویس : {serivce.Price.ToIranCurrency().En2Fa()} تومان\n" +
                    $"⌛️ مدت : <b>نامحدود</b>\n" +
                    $"👥 تعداد کاربر : <b>{serivce.UserLimit.ToString().En2Fa()} کاربره</b>\n" +
                    $"🔋 حجم : <b>نامحدود</b>\n\n" +
                    $"♻️ جهت ویرایش از منو زیر اقدام نمایید :",
                    ParseMode.Html,
                    replyMarkup: ServiceKeyboards.NewService(servicCode));

            _uw.ServiceRepository.Add(serivce);
        }
        else if (data.StartsWith("update*"))
        {
            var service = await _uw.ServiceRepository.GetServiceByCode(data.Split("*")[1]);
            var property = data.Split("*")[2];

            switch (property)
            {
                case "title":
                    await _bot.Choosed(callBackQuery);
                    await _bot.SendTextMessageAsync(user.Id, "📌 نام سرویس را ارسال کنید :",
                        replyMarkup: MarkupKeyboards.Cancel());
                    _uw.SubscriberRepository.ChangeStep(user.Id,
                        $"{Constants.ServiceConstants}-update*{service.Code}*title*{callBackQuery.Message.MessageId}");
                    break;
                case "category":
                    var categories = await _uw.ServiceCategoryRepository.GetAllCategoriesAsync();
                    if (categories.Count is not 0)
                    {
                        await _bot.Choosed(callBackQuery);
                        await _bot.SendTextMessageAsync(user.Id, "🌀 دسته بندی سرویس را انتخاب کنید :",
                            replyMarkup: ServiceKeyboards.ServiceCategories(service,
                                categories,
                                callBackQuery.Message.MessageId));
                    }
                    else
                    {
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "دسته بندی ای یافت نشد.",
                            true);
                    }

                    break;
                case "description":
                    await _bot.Choosed(callBackQuery);
                    await _bot.SendTextMessageAsync(user.Id, "💬 توضیحات سرویس را ارسال کنید :",
                        replyMarkup: MarkupKeyboards.Cancel());
                    _uw.SubscriberRepository.ChangeStep(user.Id,
                        $"{Constants.ServiceConstants}-update*{service.Code}*description*{callBackQuery.Message.MessageId}");
                    break;
                case "price":
                    await _bot.Choosed(callBackQuery);
                    await _bot.SendTextMessageAsync(user.Id, "💲 قیمت سرویس را به تومان ارسال کنید :",
                        replyMarkup: MarkupKeyboards.Cancel());
                    _uw.SubscriberRepository.ChangeStep(user.Id,
                        $"{Constants.ServiceConstants}-update*{service.Code}*price*{callBackQuery.Message.MessageId}");
                    break;
                case "duration":
                    await _bot.Choosed(callBackQuery);
                    await _bot.SendTextMessageAsync(user.Id, "⌛ مدت سرویس را به تعداد روز ارسال کنید :",
                        replyMarkup: MarkupKeyboards.Cancel());
                    _uw.SubscriberRepository.ChangeStep(user.Id,
                        $"{Constants.ServiceConstants}-update*{service.Code}*duration*{callBackQuery.Message.MessageId}");
                    break;
                case "traffic":
                    await _bot.Choosed(callBackQuery);
                    await _bot.SendTextMessageAsync(user.Id, "🔋 حجم سرویس را به گیگابایت روز ارسال کنید :",
                        replyMarkup: MarkupKeyboards.Cancel());
                    _uw.SubscriberRepository.ChangeStep(user.Id,
                        $"{Constants.ServiceConstants}-update*{service.Code}*traffic*{callBackQuery.Message.MessageId}");
                    break;
                case "userlimit":
                    await _bot.Choosed(callBackQuery);
                    await _bot.SendTextMessageAsync(user.Id,
                        "👥 تعداد کاربر سرویس را به صورت عددی ارسال کنید :",
                        replyMarkup: MarkupKeyboards.Cancel());
                    _uw.SubscriberRepository.ChangeStep(user.Id,
                        $"{Constants.ServiceConstants}-update*{service.Code}*userlimit*{callBackQuery.Message.MessageId}");
                    break;
                case "collegues":
                    await _bot.Choosed(callBackQuery);
                    var colleaguesRuleOnService =
                        await _uw.OfferRulesRepository.GetByServiceCode(service.Code);
                    if (colleaguesRuleOnService is null)
                    {
                        var newOfferRule = new OfferRule()
                        {
                            ServiceCode = service.Code,
                            LessThan5Order = 0,
                            MoreThan5Order = 0,
                            MoreThan15Order = 0
                        };
                        _uw.OfferRulesRepository.Add(newOfferRule);
                        colleaguesRuleOnService = newOfferRule;
                    }

                    await _bot.SendTextMessageAsync(user.Id, callBackQuery.Message.Text, ParseMode.Html,
                        replyMarkup: ServiceKeyboards.ColleaguesPriceSettings(colleaguesRuleOnService));
                    await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
                    break;
                case "remove":
                    service.IsRemoved = true;
                    _uw.ServiceRepository.Update(service);
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                        "سرویس با موفقیت حذف شد.", true);
                    await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
                    _bot.ServiceRemovedReport(user.Id, service, user);
                    break;
                case "done":
                    if (service.Title.HasValue() &&
                        service.Price != 0 &&
                        service.Duration != 0)
                    {
                        service.IsActive = true;
                        _uw.ServiceRepository.Update(service);
                        await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
                        _bot.NewServiceAddReport(user.Id, service, user);
                    }
                    else
                    {
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                            "پیش از ویرایش سرویس اطلاعات مورد نیاز را تکمیل نمایید.", true);
                    }

                    break;
                default:
                    break;
                    ;
            }
        }
        else if (data.StartsWith("collegueprice*"))
        {
            var type = data.Split("*")[2];

            var colleague_rules = await _uw.OfferRulesRepository.GetByServiceCode(data.Split("*")[1]);
            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, $"انتخاب شد.");

            if (type == "updatebase")
            {
                await _bot.SendTextMessageAsync(user.Id,
                    "قمت پایه این سرویس برای همکار را به تومان وارد نمایید :",
                    replyMarkup: MarkupKeyboards.Cancel());
                _uw.SubscriberRepository.ChangeStep(user.Id,
                    $"{Constants.ServiceConstants}-baseprice*{colleague_rules.Id}");
            }
            else
            {
                var action = data.Split("*")[3];
                if (action != "none")
                {
                    switch (type)
                    {
                        case "less5":
                            if (action == "plus")
                                colleague_rules.LessThan5Order++;
                            else colleague_rules.LessThan5Order--;
                            break;
                        case "more5":
                            if (action == "plus")
                                colleague_rules.MoreThan5Order++;
                            else colleague_rules.MoreThan5Order--;
                            break;
                        case "more15":
                            if (action == "plus")
                                colleague_rules.MoreThan15Order++;
                            else colleague_rules.MoreThan15Order--;
                            break;
                        default:
                            break;
                    }

                    _uw.OfferRulesRepository.Update(colleague_rules);
                    await _bot.SendTextMessageAsync(user.Id, callBackQuery.Message.Text, ParseMode.Html,
                        replyMarkup: ServiceKeyboards.ColleaguesPriceSettings(colleague_rules));
                    await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
                }
                else
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, $"این دکمه عملکردی ندارد.");
                }
            }
        }

        else if (data.StartsWith("updatecategory*"))
        {
            var service = await _uw.ServiceRepository.GetServiceByCode(data.Split("*")[1]);
            if (service is not null)
            {
                _uw.ServiceRepository.Update(service);
                await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
                await _bot.DeleteMessageAsync(user.Id, int.Parse(data.Split("*")[3]));
                await _bot.AddNewService(_uw, user.Id, service);
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "سرویس مورد نظر یافت نشد", true);
            }
        }
        else if (data.StartsWith("category*"))
        {
            var category = await _uw.ServiceCategoryRepository.GetByServiceCategoryCode(data.Split("*")[1]);
            if (category is not null)
            {
                var services = await _uw.ServiceRepository.GetServicesByCategoryCodeAsync(category.Code);
                await _bot
                    .SendTextMessageAsync(groupId,
                        $"مدیریت سرویس های {category.Title} :\n\n" +
                        $"✔️ تعداد سرویس های فعال : <b>{services.Where(s => s.IsActive).ToList().Count} عدد</b>\n" +
                        $"✖️️ تعداد سرویس های غیرفعال : <b>{services.Where(s => !s.IsActive).ToList().Count} عدد</b>\n\n" +
                        $"قصد مدیریت کدام نوع از سرویس ها را دارید؟",
                        ParseMode.Html,
                        replyMarkup: ServiceKeyboards.ServiceManagement(category, services));
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "دسته بندی سرویس یافت نشد.", true);
            }
        }
    }
}