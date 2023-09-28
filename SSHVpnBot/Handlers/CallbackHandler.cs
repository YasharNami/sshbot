using ConnectBashBot.Commons;
using SSHVpnBot.Components;
using SSHVpnBot.Components.AccountReports.Handlers;
using SSHVpnBot.Components.Accounts.Handlers;
using SSHVpnBot.Components.Checkouts;
using SSHVpnBot.Components.Discounts.Handlers;
using SSHVpnBot.Components.Locations;
using SSHVpnBot.Components.Locations.Keyboards;
using SSHVpnBot.Components.Orders.Handlers;
using SSHVpnBot.Components.Servers.Handlers;
using SSHVpnBot.Components.Servers.Keyboards;
using SSHVpnBot.Components.ServiceCategories.Handlers;
using SSHVpnBot.Components.Services;
using SSHVpnBot.Components.Services.Handlers;
using SSHVpnBot.Components.Subscribers;
using SSHVpnBot.Components.Subscribers.Handlers;
using SSHVpnBot.Components.Transactions;
using SSHVpnBot.Repositories.Uw;
using SSHVpnBot.Telegram;
using SSHVpnBot.Telegram.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Location = SSHVpnBot.Components.Locations.Location;

namespace ConnectBashBot.Telegram.Handlers;

public static class CallbackHandler
{
    public static async Task HandleCallbackQueryAsync(this ITelegramBotClient _bot, Update update, IUnitOfWork _uw,
        CancellationToken cancellationToken = default)
    {
        var callBackQuery = update.CallbackQuery!;
        var user = callBackQuery.From;
        var chatId = user.Id;
        var data = callBackQuery.Data!;
        var groupId = callBackQuery.Message.Chat.Id;

        await _uw.LoggerService.LogCallback(Program.logger_bot, update);

        var step = await _uw.SubscriberRepository.CheckStep(chatId);

        if (step == null)
        {
            var new_suscriber = await user.CreateSubscriberAsync();
            _uw.SubscriberRepository.AddWithId(new_suscriber);
            step = new_suscriber.Step;
        }

        var joined = await _bot.CheckJoin(chatId, MainHandler._mainchannel);
        var userInfo = await _uw.SubscriberRepository.GetByChatId(chatId);
        if (userInfo.isActive)
        {
            if (data.Equals("joined"))
            {
                if (joined)
                {
                    await _bot.SendTextMessageAsync(chatId,
                        "لطفا گزینه مورد نظر را از منو انتخاب نمائید :",
                        replyMarkup: MarkupKeyboards.Main(userInfo.Role));
                    _uw.SubscriberRepository.ChangeStep(chatId, "none");
                    await _bot.DeleteMessageAsync(chatId, callBackQuery.Message.MessageId);
                    await _bot.Choosed(callBackQuery);
                }
                else
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "شما هنوز عضو کانال نشده اید", true);
                }
            }
            else
            {
                if (!joined)
                {
                    await _bot.SendTextMessageAsync(chatId,
                        "پیش از ادامه کار با ربات لطفا در کانال ما عضو شوید 🙏 \n\n" +
                        "🆔 @connectbash", replyMarkup: InlineKeyboards.Joined());
                    return;
                }
            }

            var sub = data.Split("-")[0];
            switch (sub)
            {
                case Constants.SubscriberConstatns:
                    var subscriberhandler = new SubscriberCallbackHandler(_bot, update, _uw, userInfo);
                    subscriberhandler.QueryHandlerAsync();
                    break;
                case Constants.OrderConstants:
                    var orderhandler = new OrderCallbackHandler(_bot, update, _uw, userInfo);
                    orderhandler.QueryHandlerAsync();
                    break;
                case Constants.AccountConstants:
                    var accounthandler = new AccountCallbackHandler(_bot, update, _uw, userInfo);
                    accounthandler.QueryHandlerAsync();
                    break;
                case Constants.DiscountConstants:
                    var discounthandler = new DiscountCallbackHandler(_bot, update, _uw, userInfo);
                    discounthandler.QueryHandlerAsync();
                    break;
                case Constants.ServerConstants:
                    var serverhandler = new ServerCallbackHandler(_bot, update, _uw, userInfo);
                    serverhandler.QueryHandlerAsync();
                    break;
                case Constants.ServiceConstants:
                    var servicehandler = new ServiceCallbackHandler(_bot, update, _uw, userInfo);
                    servicehandler.QueryHandlerAsync();
                    break;
                case Constants.AccountReportConstants:
                    var reporthandler = new AccountReportCallbackHandler(_bot, update, _uw, userInfo);
                    reporthandler.QueryHandlerAsync();
                    break;
                case Constants.CategoryConstants:
                    var categoryhandler = new ServiceCategoryCallbackHandler(_bot, update, _uw, userInfo);
                    categoryhandler.QueryHandlerAsync();
                    break;
            }

            if (data.StartsWith("checkoutconfirm*"))
            {
                var checkout = await _uw.CheckoutRepository.GetCheckoutByCode(data.Split("*")[1]);
                if (checkout is not null)
                {
                    var answer = data.Split("*")[2];
                    await _bot.DeleteMessageAsync(chatId, callBackQuery.Message.MessageId);
                    if (answer.Equals("approve"))
                    {
                        var transaction_code = Transaction.GenerateNewDiscountNumber();
                        var transaction = new Transaction()
                        {
                            Amount = checkout.Amount - checkout.Amount * 2,
                            TransactionCode = transaction_code,
                            UserId = chatId,
                            IsRemoved = false,
                            CreatedOn = DateTime.Now,
                            Type = TransactionType.Checkout
                        };
                        _uw.TransactionRepository.Add(transaction);

                        checkout.TransactionCode = transaction.TransactionCode;
                        _uw.CheckoutRepository.Update(checkout);

                        var referrals = await _uw.SubscriberRepository.GetAllByReferral(chatId.ToString());
                        await _bot.SendTextMessageAsync(MainHandler._payments,
                            $".\n" +
                            $"🟣 درخواست برداشت وجه ثبت شد.\n\n" +
                            $"👤 <a href='tg://user?id={checkout.UserId}'>{user.FirstName + " " + user.LastName}</a> | #U{checkout.UserId}\n" +
                            $"👥 <b>{referrals.Value.En2Fa()} زیرمجموعه تا کنون</b>\n" +
                            $"💰 <b>{checkout.Amount.ToIranCurrency().En2Fa()} تومان</b>\n" +
                            $"💳 <code>IR{checkout.IBan}</code>\n" +
                            $"🕒 <b>{checkout.CreatedOn.ConvertToPersianCalendar()}</b>\n\n" +
                            $"📌 آیا واریز فوق را انجام دادید؟",
                            ParseMode.Html,
                            replyMarkup: InlineKeyboards.CheckoutAdminConfirmation(checkout.Code));

                        await _bot.SendTextMessageAsync(chatId,
                            $".\n" +
                            $"درخواست برداشت وجه شما با کد پیگیری <code>#{checkout.Code}</code> ثبت شد. ✅\n" +
                            $"اپراتوهای ما حداکثر تا <b>۱۲ ساعت</b> سفارش شما را بررسی میکنند و بعد از تایید وجه به حساب شما واریز خواهد شد.\n\n" +
                            $"در صورتی که بعد از این زمان واریزی به حسابتان صورت نگرفت می‌توانید با زدن دکمه‌ی زیر سفارش خود را پیگیری کنید",
                            ParseMode.Html,
                            replyMarkup: InlineKeyboards.TrackCheckout(checkout.Code));
                    }
                    else
                    {
                        await _bot.SendTextMessageAsync(chatId,
                            $".\n" +
                            $"درخواست برداشت وجه شما با شناسه #{checkout.Code} با موفقیت حذف شد.",
                            ParseMode.Html, replyMarkup: MarkupKeyboards.Main(userInfo.Role));
                    }
                }
                else
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "درخواست برداشت وجه یافت نشد.", true);
                }
            }
            else if (data.StartsWith("back*"))
            {
                var section = data.Split("*")[1];
                await _bot.Choosed(callBackQuery);
                switch (section)
                {
                    case "menu":
                        _uw.SubscriberRepository.ChangeStep(chatId, "none");
                        await _bot.SendTextMessageAsync(chatId, $"به منو اصلی بازگشتید.",
                            replyMarkup: MarkupKeyboards.Main(userInfo.Role));
                        break;
                }

                await _bot.DeleteMessageAsync(chatId, callBackQuery.Message.MessageId);
            }
            else if (data.StartsWith("cart*"))
            {
                var cart = _uw.ConfigurationRepository.GetById(int.Parse(data.Replace("cart*", "")));
                if (cart is not null)
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                        "اطلاعات کارت بانکی جهت ویرایش برای شما ارسال شد.", true);
                    await _bot.SendTextMessageAsync(callBackQuery.From.Id,
                        $".\n" +
                        $"💳 ویرایش اطاعات کارت بانکی\n\n" +
                        $"📍 کارت <b>{cart.Type.ToDisplay()}</b>\n" +
                        $"👤 <b>{cart.BankAccountOwner}</b>\n" +
                        $"📌 <b>{cart.CardNumber.En2Fa()}</b>\n\n" +
                        $"قصد ویرایش کدام یک از اطلاعات را دارید؟",parseMode: ParseMode.Html,
                        replyMarkup: InlineKeyboards.SingleCartMangement(cart));
                }
                else
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "کارت مورد نظر یافت نشد.", true);
                }
            }
            else if (data.StartsWith("updatecart*"))
            {
                var cart = _uw.ConfigurationRepository.GetById(int.Parse(data.Split("*")[2]));
                if (cart is not null)
                    switch (data.Split("*")[1])
                    {
                        case "owner":
                            await _bot.Choosed(callBackQuery);
                            await _bot.SendTextMessageAsync(chatId,
                                $"👤 نام و نام خانوادگی صاحب کارت را وارد کنید :",
                                replyMarkup: MarkupKeyboards.Cancel());
                            _uw.SubscriberRepository.ChangeStep(chatId,
                                $"updatecart*owner*{cart.Id}*{callBackQuery.Message.MessageId}");
                            break;
                        case "number":
                            await _bot.Choosed(callBackQuery);
                            await _bot.SendTextMessageAsync(chatId, $"💳 شماره کارت را وارد کنید :",
                                replyMarkup: MarkupKeyboards.Cancel());
                            _uw.SubscriberRepository.ChangeStep(chatId,
                                $"updatecart*number*{cart.Id}*{callBackQuery.Message.MessageId}");
                            break;
                        case "done":
                            _uw.SubscriberRepository.ChangeStep(chatId, "none");
                            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "کارت بانکی با موفقیت ویرایش شد.",
                                true);
                            await _bot.SendTextMessageAsync(MainHandler._managementgroup,
                                $"شماره کارت : \n" +
                                $"💳 <b>{cart.CardNumber.En2Fa()}</b>\n" +
                                $"به نام : \n" +
                                $"👤 <b>{cart.BankAccountOwner}</b>\n\n" +
                                $"شماره کارت <b>{cart.Type.ToDisplay()}</b> با موفقیت ویرایش شد.✅",
                                ParseMode.Html);
                            await _bot.DeleteMessageAsync(chatId, callBackQuery.Message.MessageId);
                            break;
                    }
                else await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "کارت مورد نظر یافت نشد.", true);
            }
            else if (data.StartsWith("representation*"))
            {
                await _bot.Choosed(callBackQuery);
                switch (data.Replace("representation*", ""))
                {
                    case "bronze":
                        await _bot.SendTextMessageAsync(chatId,
                            $"<b>🥉 سیستم فروش برنزی :</b>\n\n" +
                            $"📌 تنها با پرداخت ماهیانه <b>هشتصد هزار تومان</b> بصورت اختصاصی ربات انحصاری خود را در اختیار مشتریان و همکاران خود بگذارید.\n\n\nامکانات و مزایای سیستم :\n\n▫️امکان تغییر محتوا ربات\n▫️فروش تک و عمده به مشتریان یا همکاران\n▫️تعریف و مدیریت اشتراک ها\n▫️تعریف و مدیریت سرور ها\n▫️پرداخت از طریق درگاه و کارت به کارت\n▫️مدیریت روش های پرداخت\n▫️اتصال به حداکثر ۲۰ پنل وی تو ری بصوت همزمان\n▫️دریافت گزارش فعالیت های اخیر\n▫️رابط کاربری بی نظیر\n▫️مدیریت کاربر، آمار گیری هوشمند و...\n▫️قابلیت تنظیم سرویس تستی برای کاربران\n▫️تحویل زیر ۵ ثانیه کانفینگ در هر ساعت شبانه روز\n▫️امکان گزارش کندی و قطعی برای مشتریان\n▫️امکان ارسال پیام همگانی\n\n\n🔻 سفارشی سازی ربات خود مطابق با نام فروشگاه شما\n\n🔻 تحویل سیستم به شما در کمتر از ۲۴ ساعت\n\n🔻 پشتیبانی سیستم در هر ساعت از شبانه روز\n\n🔻 ارایه مشاوره در رابطه با تبلیغات و ارایه مستندات و آموزش های لازم کار با ربات\n\n\n🔗 در صورت دریافت مشاوره و سفارش به آیدی زیر مراجعه کنید : \n\n🧑‍💻 @connect_bash\n\n.",
                            ParseMode.Html);
                        break;
                    case "silver":
                        await _bot.SendTextMessageAsync(chatId,
                            $"<b>🥈 سیستم فروش نقره ای :</b>\n\n\n📌 تنها با پرداخت ماهیانه <b>یک میلیون و دویصد هزار تومان</b> بصورت اختصاصی ربات انحصاری خود را در اختیار مشتریان و همکاران خود بگذارید.\n\n\nامکانات و مزایای سیستم :\n\n▫️تمام امکانات سیستم برنزی\n▫️امکان پرداخت ارزی\n▫️امکان پرداخت بصورت پرفکت مانی\n▫️سیستم کد تخفیف\n▫️سیستم گزارش گیری کامل\n▫️سیستم لود بالانسر و مدیریت منابع، امکان جابجایی کانفیگ ها از یک سرور به سرور دیگر و به صفر رساندن اختلالات و کندی ها.\n▫️امکان ارسال پیام همگانی تفکیک شده\n\n\n🔻 سفارشی سازی ربات خود مطابق با نام فروشگاه شما\n\n🔻 تحویل سیستم به شما در کمتر از ۲۴ ساعت\n\n🔻 پشتیبانی سیستم در هر ساعت از شبانه روز\n\n🔻 ارایه مشاوره در رابطه با تبلیغات و ارایه مستندات و آموزش های لازم کار با ربات\n\n\n🔗 در صورت دریافت مشاوره و سفارش به آیدی زیر مراجعه کنید : \n\n🧑‍💻 @connect_bash\n\n.",
                            ParseMode.Html);
                        break;
                    case "golden":
                        await _bot.SendTextMessageAsync(chatId,
                            $"<b>🏅 سیستم فروش طلایی :</b>\n\n\n📌 تنها با پرداخت <b>هزینه توافقی</b> بصورت اختصاصی ربات انحصاری خود را در اختیار مشتریان و همکاران خود بگذارید.\n\n\nامکانات و مزایای سیستم :\n\n▫️تمام امکانات سیستم نقره ای را شامل خواهد شد.\n▫️عدم نگرانی بابت تامین سرور ها (استفاده از زیرساخت و سرور های {MainHandler.persianTitle})\n▫️ارتباط مستقیم با تیم برنامه نویسی مجموعه و پیاده سازی آپدیت سفارشی روی سیستم و افزودن امکانات دیگر مطابق با نیاز شما.\n\n\n🔻 سفارشی سازی ربات خود مطابق با نام فروشگاه شما\n\n🔻 تحویل سیستم به شما در کمتر از ۲۴ ساعت\n\n🔻 پشتیبانی سیستم در هر ساعت از شبانه روز\n\n🔻 ارایه مشاوره در رابطه با تبلیغات و ارایه مستندات و آموزش های لازم کار با ربات\n\n\n🔗 در صورت دریافت مشاوره و سفارش به آیدی زیر مراجعه کنید : \n\n🧑‍💻 @connect_bash\n\n.",
                            ParseMode.Html);
                        break;
                }
            }
            else if (data.StartsWith("help*"))
            {
                var type = data.Replace("help*", "");

                await _bot.DeleteMessageAsync(chatId, callBackQuery.Message.MessageId);
                if (type.Equals("lowspeed"))
                    await _bot.SendTextMessageAsync(chatId,
                        "ر صورتی که سرویس شما با کندی سرعت و یا اختلال روبروست، لطفا پیش از اینکه مستقیماً به ادمین ها پیامی ارسال کنید، از طریق ربات گزارش کندی و اختلال ثبت کنید. به این شکل، کانفیگ شما به صورت کامل و با مشخصات فنی برای تیم فنی ارسال خواهد شد، و نیازی به ارسال کانفیگ توسط شما به تیم فنی نخواهد بود.\n\n✅ راهنمای ثبت اختلال برای کاربران:\n\nاز بخش «🧩 مدیریت سرویس‌ها» سرویسی که با مشکل روبروست را انتخاب کنید، پس از مشاهده وضعیت کانفیگ و اطمینان از عدم انقضاء/اتمام حجم و...  ، بر روی «🐢 گزارش کندی و اختلال» کلیک کنید.\n\n✅ راهنمای ثبت اختلال برای فروشندگان:\n\nاز بخش «🔎 بررسی وضعیت» شناسه کانفیگی که با مشکل روبروست را وارد نمائید، پس از مشاهده وضعیت کانفیگ و اطمینان از عدم انقضاء/اتمام حجم و... ، بر روی «🐢 گزارش کندی و اختلال» کلیک کنید.\n\n⚠️ توجه: لطفاً پیش از ثبت گزارش، به اطلاع رسانی های انجام شده در کانال توجه فرمائید. همچنین بلافاصله پس از ثبت گزارش، از ادمین ها پیگیری نفرمائید تا فرصت بررسی و رفع مشکل از سمت تیم فنی وجود داشته باشد.\n\nبا تشکر 🙏"
                        ,parseMode: ParseMode.Html,
                        replyMarkup: InlineKeyboards.BackHelp());
                else if (type.Equals("extend"))
                    await _bot.SendTextMessageAsync(chatId,
                        "🔆 چنانچه حجم و یا زمان اشتراک شما به اتمام رسیده باشد و کانفیگ شما غیر فعال شده باشد، امکان تمدید سرویس توسط شما براحتی مقدور است.\n\n✅ راهنمای تمدید سرویس برای کاربران:\n\nاز بخش «🧩 مدیریت سرویس‌ها» سرویسی که منقضی شده است را انتخاب کنید، پس از مشاهده وضعیت کانفیگ و اطمینان از انقضاء و یا اتمام حجم ترافیک ، بر روی «⌛️تمدید سرویس» کلیک کنید.\n\n✅ راهنمای تمدید سرویس برای فروشندگان:\n\nاز بخش «🔎 بررسی وضعیت» سرویسی که منقضی شده است را انتخاب کنید، پس از مشاهده وضعیت کانفیگ و اطمینان از انقضاء و یا اتمام حجم ترافیک ، بر روی «⌛️تمدید سرویس» کلیک کنید.\n\n⚠️ توجه: تا 1 هفته پس از انقضاء زمان و اتمام حجم امکان تمدید سرویس وجود دارد. پس از گذشت این زمان، کانفیگ مورد نظر از سرور حذف گردیده و امکان تمدید آن وجود نخواهد داشت و باید سرویس جدیدی تهیه بفرمائید.\n\nبا تشکر 🙏"
                        ,parseMode: ParseMode.Html,
                        replyMarkup: InlineKeyboards.BackHelp());
                else if (type.Equals("block"))
                    await _bot.SendTextMessageAsync(chatId,
                        "🔅 اگر شما پیامی حاوی «اخطار» یا «مسدودی» اکانت از ربات دریافت کرده‌اید، به این معناست که سیستم روی کانفیگ خریداری شده توسط شما تقلب تشخیص داده است!\n\n🚫 لطفا از اشتراک گذاری کانفیگ‌ها یا اتصال بیشتر از تعداد مجاز به یک اکانت خودداری کنید، این خلاف قوانین ماست و در صورتی که اکانت شما مسدود شود به هیچ‌ وجه امکان تعویض یا فعال کردن مجدد آن وجود ندارد و هیچ‌ مبلغی نیز بازگشت داده نخواهد شد.\n\n❗️جهت بررسی مسدود بودن یک اکانت، کافیست از قسمت مدیریت سرویس (برای کابران) و بررسی وضعیت (برای فروشندگان) ، وضعیت سرویس را مشاهده کنید. در صورت مسدود شدن، در مقابل وضعیت اشتراک عبارت \"مسدود 🛑\" قابل مشاهده است.\n\n⚠️ توجه: لطفا برای رفع مسدودی یا اخطار به ادمین پیام‌ ندهید، ادمین‌ ها دسترسی این‌ کار را ندارند چون این پروسه کاملا اتوماتیک است و توسط ربات انجام می شود.\n\nبا تشکر 🙏",
                        ParseMode.Html,
                        replyMarkup: InlineKeyboards.BackHelp());
                else if (type.Equals("rules"))
                    await _bot.SendTextMessageAsync(chatId,
                        "🔰 شرایط و قوانین:\n\n⭕️ سرویس های ما صرفاً جهت دور زدن تحریم های اینترنتی ارائه می گردد و عواقب هرگونه سوء استفاده احتمالی از سرویس ها (فعالیت های سیاسی و فعالیت های مغایر امنیت ملی و...) بر عهده مصرف کننده نهایی می باشد.\n\n⭕️ با توجه به افزایش فیلترینگ و محدودیت اپراتورها، چنانچه اختلالی در سرورها ایجاد شود کاملاً طبیعیست. و ادعای ارائه سرویس ۱۰۰٪ بدون قطعی توسط هر شخصی صرفاً جهت افزایش فروش ناجوانمردانه و کلاهبرداری می باشد.\n\n⭕️ گرچه تیم فنی و پشتیبانی تا به امروز موفق بوده و گزارش های قطعی و اختلال در اسرع وقت بررسی و رفع گردیده است، ولی شایان توجه است که هر لحظه احتمال اختلال در سرورها وجود دارد و مدت زمان رفع اختلال نیز بستگی به شرایط همان زمان (از ۱۵ دقیقه تا حداکثر ۱۲ ساعت) خواهد داشت. لذا با در نظر داشتن این مهم اقدام به سفارش نمائید.\n\n⭕️ اشتراک گذاری کانفیگ های دریافتی از ربات و اتصال بیشتر از تعداد مجاز به یک اکانت موجب مسدودی سرویس خریداری شده خواهد شد و ما هیچگونه مسئولیتی در قبال اکانت های مسدودی نخواهیم داشت.\n\n\nبا سپاس 🙏"
                        ,parseMode: ParseMode.Html,
                        replyMarkup: InlineKeyboards.BackHelp());
                else if (type.Equals("contact"))
                    await _bot.SendTextMessageAsync(chatId,
                        "❓ برخی سوالات متداول و پرتکرار کاربران:\n\n📌سوال: منظور از تک/دو کاربره چیست؟\n\nپاسخ: منظور این است که فقط یک/دو دستگاه می‌توانند به صورت همزمان به این کانفیگ متصل شوند. در صورت وجود اتصال بیش از تعداد مجاز روی یک کانفیگ، کانفیگ شما مسدود خواهد شد و هیچ‌ مبلغی برگشت داده نخواهد شد.\n\n📌سوال: من ۳ دستگاه دارم (مثلا یک تلوزیون، یک لپ تاپ و یک گوشی)، آیا می‌توانم با یک اکانت هر ۳ را متصل کنم؟\n\nپاسخ: در صورتی که هیچ لحظه‌ای وجود نداشته باشد که ۳ دستگاه همزمان به سرور اتصال داشته باشند مشکلی ندارد و می‌توانید استفاده کنید، ولی مثلا اگر فراموش کنید که اتصال تلوزیون خود را قطع کنید و سپس از گوشی خود استفاده کنید امکان دارد کانفیگ شما مسدود شود و عذری پذیرفته نیست.\n\n📌 سوال: آیا میتوانم کانفیگ شش‌ ماهه یا یک‌ ساله تهیه کنم؟\n\nپاسخ: در حاضر با توجه به اینکه وضعیت اینترنت کشور استیبل نیست، ما چنین سرویس‌هایی را ارائه نمیدهیم چرا که مشخص نیست بتوانیم تا آن زمان از شما پشتیبانی کنیم.\n\n——————————————————-—\n\n🔅 اگر سوال شما جز هیچ‌ کدام از سوالات بالا نبود، می‌توانید با ادمین‌های ما به صورت مستقیم ارتباط بگیرید:"
                        ,parseMode: ParseMode.Html,
                        replyMarkup: InlineKeyboards.BackHelpContact());
            }
            else if (data.Equals("backhelp"))
            {
                await _bot.DeleteMessageAsync(chatId, callBackQuery.Message.MessageId);
                await _bot.SendTextMessageAsync(chatId,
                    "📚 در چه خصوصی نیاز به پشتیبانی دارید؟", replyMarkup: InlineKeyboards.Help());
            }
            else if (data.Equals("marketing-management"))
            {
                await _bot.SendTextMessageAsync(groupId,
                    $".\n" +
                    $"قصد اقدام به کدام فعالیت مارکتینگی دارید؟"
                    ,parseMode: ParseMode.Html
                    , replyMarkup: InlineKeyboards.Marketings());
            }
            else if (data.StartsWith("marketing*"))
            {
                var kind = data.Split("*")[1];
                var services = await _uw.ServiceRepository.GetAllPublicsAsync();

                switch (kind)
                {
                    case "todaynotpaidorders":
                        await _bot.Choosed(callBackQuery);
                        var not_paids = await _uw.OrderRepository.TodayNotPaids();
                        await _bot.SendTextMessageAsync(groupId,
                            $".\n" +
                            $"🔖 لیست فاکتور های پرداخت نشده امروز :\n\n" +
                            $"➕ <b>{not_paids.Count.En2Fa()}</b> فاکتور پرداخت نشده\n" +
                            $"💸 <b>{not_paids.Sum(s => s.TotalAmount).ToIranCurrency().En2Fa()}</b> تومان پرید\n" +
                            $"👥 ایجاد شده توسط <b>{not_paids.GroupBy(s => s.UserId).Count().En2Fa()}</b> کاربر\n" +
                            $".",
                            ParseMode.Html);
                        if (not_paids.Count >= 20)
                        {
                            for (var page = 0; page < Math.Ceiling((decimal)not_paids.Count / 20); page++)
                            {
                                var ids = "";
                                foreach (var order in not_paids.Skip(page * 20).Take(20)
                                             .OrderByDescending(s => s.CreatedOn).ToList())
                                {
                                    var time_differ = DateTime.Now - order.CreatedOn;
                                    var role = await _uw.SubscriberRepository.GetRoleAsync(order.UserId);
                                    var service = services.FirstOrDefault(s => s.Code.Equals(order.ServiceCode));
                                    ids += $"🔖 <code>#{order.TrackingCode}</code>\n" +
                                           $"{(role == Role.Colleague ? "🧑‍💻" : "👤")} <a href='tg://user?id={order.UserId}'>{order.UserId}</a>\n" +
                                           $"{(service is not null ? $"<b>🧩 {order.Count.En2Fa()} کانفیگ {service.GetFullTitle()}</b>\n" : "")}" +
                                           $"💸 <b>{order.TotalAmount.ToIranCurrency().En2Fa()} تومان</b>\n" +
                                           $"🕓 <b>{(time_differ.TotalMinutes < 60 ? $"{((int)time_differ.TotalMinutes).ToString().En2Fa()} دقیقه پیش" : $"{((int)time_differ.TotalHours).ToString().En2Fa()} ساعت پیش")}</b>\n\n\n";
                                }

                                await _bot.SendTextMessageAsync(groupId,
                                    $".\n\n" +
                                    $"🗂 صفحه <b>{(page + 1).En2Fa()}</b> از <b>{Math.Ceiling((decimal)not_paids.Count / 20).En2Fa()}</b> \n\n" +
                                    $"{ids}" +
                                    $".",
                                    ParseMode.Html);
                                Thread.Sleep(1000);
                            }
                        }
                        else
                        {
                            var ids = "";
                            foreach (var order in not_paids.OrderByDescending(s => s.CreatedOn).ToList())
                            {
                                var time_differ = DateTime.Now - order.CreatedOn;
                                var role = await _uw.SubscriberRepository.GetRoleAsync(order.UserId);
                                var service = services.FirstOrDefault(s => s.Code.Equals(order.ServiceCode));
                                ids += $"🔖 <code>#{order.TrackingCode}</code>\n" +
                                       $"{(role == Role.Colleague ? "🧑‍💻" : "👤")} <a href='tg://user?id={order.UserId}'>{order.UserId}</a>\n" +
                                       $"{(service is not null ? $"<b>🧩 {order.Count.En2Fa()} کانفیگ {service.GetFullTitle()}</b>\n" : "")}" +
                                       $"💸 <b>{order.TotalAmount.ToIranCurrency().En2Fa()} تومان</b>\n" +
                                       $"🕓 <b>{(time_differ.TotalMinutes < 60 ? $"{((int)time_differ.TotalMinutes).ToString().En2Fa()} دقیقه پیش" : $"{((int)time_differ.TotalHours).ToString().En2Fa()} ساعت پیش")}</b>\n\n\n";
                            }

                            await _bot.SendTextMessageAsync(groupId,
                                $".\n\n" +
                                $"{ids}" +
                                $".",
                                ParseMode.Html);
                        }

                        break;
                    case "lessthanweekaccounts":
                        await _bot.Choosed(callBackQuery);
                        var accounts = await _uw.AccountRepository.GetLessThanWeekAccountsAsync();
                        await _bot.SendTextMessageAsync(groupId,
                            $".\n" +
                            $"🔗 لیست اشتراک های کمتر از یک هفته مانده به منقضی شدن :\n\n" +
                            $"➕ <b>{accounts.Count.En2Fa()}</b> اشتراک در مجموع\n" +
                            $"👥 <b>{accounts.GroupBy(s => s.UserId).Count().En2Fa()}</b> کاربر در مجموع\n",
                            ParseMode.Html);
                        var colleague_offers = _uw.OfferRulesRepository.GetAll();
                        if (accounts.Count >= 20)
                        {
                            for (var page = 0; page < Math.Ceiling((decimal)accounts.Count / 20); page++)
                            {
                                var ids = "";
                                foreach (var account in accounts.Skip(page * 20).Take(20)
                                             .OrderByDescending(s => s.EndsOn).ToList())
                                {
                                    var role = await _uw.SubscriberRepository.GetRoleAsync(account.UserId);
                                    var service = services.FirstOrDefault(s => s.Code.Equals(account.ServiceCode));
                                    var price = service?.Price ?? 0m;
                                    if (role.Equals(role == Role.Colleague))
                                        if (service is not null)
                                            price = colleague_offers.FirstOrDefault(s =>
                                                s.ServiceCode.Equals(service.Code)).BasePrice;
                                    ids += $"🔗 <code>{account.ClientId}</code>\n" +
                                           $"{(role == Role.Colleague ? "🧑‍💻" : "👤")} <a href='tg://user?id={account.UserId}'>{account.UserId}</a>\n" +
                                           $"{(service is not null ? $"<b>🧩 {service.GetFullTitle()} | {price.ToIranCurrency().En2Fa()} تومان</b>\n" : "")}" +
                                           $"🕓 <b>{account.EndsOn.ConvertToPersianCalendar()}</b>\n\n\n";
                                }

                                await _bot.SendTextMessageAsync(groupId,
                                    $".\n\n" +
                                    $"🗂 صفحه <b>{(page + 1).En2Fa()}</b> از <b>{Math.Ceiling((decimal)accounts.Count / 20).En2Fa()}</b> \n\n" +
                                    $"{ids}" +
                                    $".",
                                    ParseMode.Html);
                                Thread.Sleep(1000);
                            }
                        }
                        else
                        {
                            var ids = "";
                            foreach (var account in accounts.OrderByDescending(s => s.EndsOn).ToList())
                            {
                                var role = await _uw.SubscriberRepository.GetRoleAsync(account.UserId);
                                var service = services.FirstOrDefault(s => s.Code.Equals(account.ServiceCode));
                                var price = service?.Price ?? 0m;
                                if (role.Equals(role == Role.Colleague))
                                    if (service is not null)
                                        price = colleague_offers.FirstOrDefault(s =>
                                            s.ServiceCode.Equals(service.Code)).BasePrice;
                                ids += $"🔗 <code>{account.ClientId}</code>\n" +
                                       $"{(role == Role.Colleague ? "🧑‍💻" : "👤")} <a href='tg://user?id={account.UserId}'>{account.UserId}</a>\n" +
                                       $"{(service is not null ? $"<b>🧩 {service.GetFullTitle()} | {price.ToIranCurrency().En2Fa()} تومان</b>\n" : "")}" +
                                       $"🕓 <b>{account.EndsOn.ConvertToPersianCalendar()}</b>\n\n\n";
                            }

                            await _bot.SendTextMessageAsync(groupId,
                                $".\n\n" +
                                $"{ids}" +
                                $".",
                                ParseMode.Html);
                        }

                        break;
                }
            }

            if (userInfo.Role == Role.Admin || userInfo.Role == Role.Owner)
            {
                if (data.EndsWith("*back"))
                {
                    await _bot.Choosed(callBackQuery);

                    if (callBackQuery.Message.Chat.Type == ChatType.Private)
                        await _bot.DeleteMessageAsync(chatId, callBackQuery.Message.MessageId, cancellationToken);
                    else
                        await _bot.DeleteMessageAsync(groupId, callBackQuery.Message.MessageId, cancellationToken);
                }
              
                else if (data.Equals("payment-updatecart"))
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "انتخاب شد.");
                    var carts = _uw.ConfigurationRepository.GetAll();
                    await _bot.SendTextMessageAsync(groupId,
                        $"قصد ویرایش  اطلاعات کدام کارت را دارید ؟",
                        ParseMode.Html,
                        replyMarkup: InlineKeyboards.PaymentTypeCarts(carts));
                }
                else if (data.Equals("locations-management"))
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "مدیریت موقعیت های مکانی انتخاب شد.");
                    var locations = _uw.LocationRepository.GetAll();

                    await _bot.SendTextMessageAsync(groupId,
                        $"🏳️ قصد مدیریت کدام موقعیت مکانی را دارید؟",
                        ParseMode.Html,
                        replyMarkup: LocationKeyboards.LocationManagement(locations));
                }
                else if (data.StartsWith("add-new-location"))
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                        "اطلاعات موقعیت مکانی جدید جهت ویرایش برای شما ارسال شد.✔️", true);

                    var locationCode = Location.GenerateNewCode();
                    var location = new Location()
                    {
                        Code = locationCode,
                        CreatedOn = DateTime.Now,
                        IsActive = false,
                        Flat = "🏳️"
                    };

                    await _bot.AddNewLocation(chatId, location);
                    _uw.LocationRepository.Add(location);
                }
                else if (data.StartsWith("location*"))
                {
                    var location = await _uw.LocationRepository.GetLocationByCode(data.Split("*")[1]);
                    if (location is not null)
                    {
                        _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                            "اطلاعات موقعیت مکانی جهت ویرایش برای شما ارسال شد.", true);

                        await _bot.AddNewLocation(chatId, location);
                    }
                    else
                    {
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "موقعیت مکانی مورد نظر یافت نشد.",
                            true);
                    }
                }
                else if (data.StartsWith("updatelocation*"))
                {
                    var location = await _uw.LocationRepository.GetLocationByCode(data.Split("*")[1]);
                    if (location is not null)
                    {
                        var property = data.Split("*")[2];
                        switch (property)
                        {
                            case "title":
                                await _bot.Choosed(callBackQuery);
                                await _bot.SendTextMessageAsync(chatId,
                                    $"🏳️ نام کشور موقعیت مکانی را وارد گنید :",
                                    replyMarkup: MarkupKeyboards.Cancel());

                                _uw.SubscriberRepository.ChangeStep(chatId,
                                    $"{data}*{callBackQuery.Message.MessageId}");
                                break;
                            case "flat":
                                await _bot.Choosed(callBackQuery);
                                await _bot.SendTextMessageAsync(chatId,
                                    $"🏳️ پرجم کشور موقعیت مکانی را ارسال گنید :",
                                    replyMarkup: MarkupKeyboards.Cancel());
                                _uw.SubscriberRepository.ChangeStep(chatId,
                                    $"{data}*{callBackQuery.Message.MessageId}");
                                break;
                            case "done":
                                if (location.Title.HasValue() && location.Flat.HasValue())
                                {
                                    location.IsActive = true;
                                    _uw.LocationRepository.Update(location);

                                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                                        "موقعیت مکانی با موفقیت افزوده/ویرایش شد.", true);

                                    await _bot.DeleteMessageAsync(chatId, callBackQuery.Message.MessageId);
                                    await _bot.SendTextMessageAsync(MainHandler._managementgroup,
                                        $"<b>موقعیت مکانی به شرح زیر توسط {user.FirstName + " " + user.LastName} ویرایش/اضافه شد.✔️</b>\n\n" +
                                        $"🔖 شناسه موقعیت مکانی : <b>#{location.Code}</b>\n" +
                                        $"📌 نام موقعیت مکانی :\n" +
                                        $"<b>{location.Title}</b>\n" +
                                        $"🏳️ پرچم : {location.Flat}\n",
                                        ParseMode.Html
                                    );
                                }
                                else
                                {
                                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                                        "پیش از افزودن اطلاعات موفعیت مکانی را تکمیل کنید.", true);
                                }

                                break;
                        }
                    }
                    else
                    {
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "موقعیت مکانی مورد نظر یافت نشد.",
                            true);
                    }
                }
                else if (data.Equals("payment-management"))
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "مدیریت روش های پرداخت انتخاب شد.");

                    var methods = _uw.PaymentMethodRepository.GetAll();
                    await _bot.SendTextMessageAsync(groupId,
                        $"🔻 از این بخش میتوانید روش های پرداخت سیستم را مدیریت نمایید :",
                        ParseMode.Html,
                        replyMarkup: InlineKeyboards.PaymentMethodsManaement(methods));
                }
                else if (data.StartsWith("paymenttype*"))
                {
                    var payment_type =
                        await _uw.PaymentMethodRepository.GetPaymentType(int.Parse(data.Split("*")[1]));
                    if (data.Split("*")[2] == "active")
                    {
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                            $"{payment_type.Title} با موفقیت غیرفعال شد.",
                            true);
                        await _uw.PaymentMethodRepository.Disable(payment_type.Id);
                    }
                    else
                    {
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                            $"{payment_type.Title} با موفقیت فعال شد.",
                            true);
                        await _uw.PaymentMethodRepository.Enable(payment_type.Id);
                    }

                    var payment_methods = _uw.PaymentMethodRepository.GetAll();
                    await _bot.DeleteMessageAsync(chatId, callBackQuery.Message.MessageId);
                    await _bot.SendTextMessageAsync(groupId,
                        $"🔻 از این بخش میتوانید روش های پرداخت سیستم را مدیریت نمایید :",
                        ParseMode.Html,
                        replyMarkup: InlineKeyboards.PaymentMethodsManaement(payment_methods));
                }
                else if (data.StartsWith("admincheckoutcnofirm*"))
                {
                    var checkout = await _uw.CheckoutRepository.GetCheckoutByCode(data.Split("*")[1]);
                    if (checkout is not null)
                    {
                        var answer = data.Split("*")[2];
                        if (answer.Equals("approve"))
                        {
                            if (checkout.State == CheckoutState.Pending)
                                await _bot.ApproveNewCheckout(_uw, checkout, user, callBackQuery);
                            else
                                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                                    $"درخواست برداشت وجه در وضعیت {checkout.State.ToDisplay()} قرار دارد.",
                                    true);
                        }
                        else
                        {
                            await _bot.DeclineCheckout(_uw, checkout, user, callBackQuery);
                        }
                    }
                    else
                    {
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "درخواست برداشت یافت نشد.",
                            true);
                    }
                }
            }
        }
    }
}