using ConnectBashBot.Commons;
using ConnectBashBot.Telegram.Handlers;
using SSHVpnBot.Components.Accounts;
using SSHVpnBot.Components.Base;
using SSHVpnBot.Components.Colleagues;
using SSHVpnBot.Components.Orders.Keyboards;
using SSHVpnBot.Components.Payments;
using SSHVpnBot.Components.Subscribers.Keyboards;
using SSHVpnBot.Repositories.Uw;
using SSHVpnBot.Services.Excel.Models;
using SSHVpnBot.Telegram;
using SSHVpnBot.Telegram.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Components.Subscribers.Handlers;

public class SubscriberCallbackHandler : QueryHandler
{
    public SubscriberCallbackHandler(ITelegramBotClient _bot, Update update,
        IUnitOfWork _uw,
        Subscriber subscriber, CancellationToken cancellationToken = default)
        :base(_bot,update,_uw,subscriber)
    {
        
    }
    public override async Task QueryHandlerAsync()
    {
        var chatId = user.Id;
        if (data.Equals("chargewallet"))
        {
            var blanace = await _uw.TransactionRepository.GetMineBalanceAsync(chatId);
            await _bot.Choosed(callBackQuery);
            var msg = await _bot.SendTextMessageAsync(chatId, $"⏳",
                replyMarkup: MarkupKeyboards.Cancel());
            await _bot.DeleteMessageAsync(chatId, msg.MessageId);
            await _bot.SendTextMessageAsync(chatId,
                $".\n" +
                $"💰 موجودی شما : <b>{blanace.Value.ToIranCurrency().En2Fa()} تومان</b>\n\n" +
                $"<b>📥 مقدار شارژ تومانی خود را انتخاب یا وارد کنید :</b>",
                ParseMode.Html,
                replyMarkup: SubscriberKeyboards.ChargeAmounts());
            _uw.SubscriberRepository.ChangeStep(chatId, $"{Constants.SubscriberConstatns}-chargewallet");
        }
        else if (data.StartsWith("chargeamount*"))
        {
            var amount = decimal.Parse(data.Split("*")[1]);
            var payment_code = Payment.GenerateNewPaymentCode();
            var msg = await _bot.SendTextMessageAsync(chatId, $"✅",
                replyMarkup: MarkupKeyboards.Main(subscriber.Role));
            await _bot.DeleteMessageAsync(chatId, msg.MessageId);
            var payment = new Payment()
            {
                Amount = amount,
                State = PaymentState.Pending,
                CreatedOn = DateTime.Now,
                PaymentCode = payment_code,
                IsRemoved = false,
                UserId = chatId
            };
            _uw.PaymentRepository.Add(payment);

            var configs = _uw.ConfigurationRepository.GetById(1);
            payment.Type = PaymentType.Cart;
            await _bot.SendTextMessageAsync(chatId,
                $".\n" +
                $"جهت شارژ حساب لطفا مبلغ <b>{payment.Amount.ToIranCurrency().En2Fa()} تومان</b>\n" +
                $"به شماره کارت زیر به نام <b>«{configs.BankAccountOwner}»</b> واریز فرمایید:\n" +
                $"💳 <code>{configs.CardNumber}</code>\n\n" +
                $"سپس روی دکمه‌ی «ارسال تصویر فیش» کلیک کنید." +
                $"حساب شما بلافاصله پس از تایید فیش شارژ خواهد شد.✔️",
                ParseMode.Html,
                replyMarkup: OrderKeyboards.SendPaymentRecept(payment_code));
            _uw.SubscriberRepository.ChangeStep(chatId, "none");
        }
        else if (data.Equals("checkoutreferral"))
        {
            var balance = await _uw.TransactionRepository.GetMineReferralBalance(chatId);
            if (balance >= 200000)
            {
                await _bot.Choosed(callBackQuery);
                await _bot.SendTextMessageAsync(chatId,
                    $".\n" +
                    $"جهت تسویه مبلغ <b>{balance.Value.ToIranCurrency().En2Fa()} تومان</b>\n" +
                    $"لطفا یک شماره شبا جهت واریز ارسال کنید :"
                    , ParseMode.Html, replyMarkup: MarkupKeyboards.Cancel());
                _uw.SubscriberRepository.ChangeStep(chatId, "sendiban");
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                    $"حداقل میزان جهت تسویه حساب ۲۰۰،۰۰۰ تومان می باشد.",
                    true);
            }
        }
        else if (data.StartsWith("tocolleague"))
        {
            var section = data.Split("*")[1];
            var colleague = await _uw.ColleagueRepository.GetByChatId(chatId);
            if (colleague is not null)
            {  
                await _bot.DeleteMessageAsync(chatId, callBackQuery.Message.MessageId);

                switch (section)
                {
                    case "howtosell":
                        colleague.HowToSell = Enum.Parse<HowToSell>(data.Split("*")[2]);
                        colleague.Stage = ColleagueStage.AverageOrder;
                        _uw.ColleagueRepository.Update(colleague);
                        await _bot.SendTextMessageAsync(subscriber.UserId,
                            $"🔑 در حال تکمیل مراحل درخواست همکاری :\n" +
                            $"🔖 #U<code>{colleague.UserId}</code>\n" +
                            $"📌 مرحله چهارم از پنجم\n\n" +
                            $"✔️ میزان فروش کانفیگ شما بصورت میانگین چه تعداد در روز است؟",
                            ParseMode.Html,
                            replyMarkup: SubscriberKeyboards.ColleagueAverageOrder());
                        break;
                    case "averageorder":
                        colleague.AverageOrder =  Enum.Parse<AverageOrder>(data.Split("*")[2]);
                        colleague.Stage = ColleagueStage.HowMeetUs;
                        _uw.ColleagueRepository.Update(colleague);
                        await _bot.SendTextMessageAsync(subscriber.UserId,
                            $"🔑 در حال تکمیل مراحل درخواست همکاری :\n" +
                            $"🔖 #U<code>{colleague.UserId}</code>\n" +
                            $"📌 مرحله پنجم از پنجم\n\n" +
                            $"✔️ از چه طریق با وصل باش آشنا شده اید؟",
                            ParseMode.Html,
                            replyMarkup: SubscriberKeyboards.ColleagueHowToMeetUs());
                        break;
                    case "howtomeetus":
                        colleague.HowMeetUs =  Enum.Parse<HowMeetUs>(data.Split("*")[2]);
                        colleague.Stage = ColleagueStage.Done;
                        _uw.ColleagueRepository.Update(colleague);
                      
                        await _bot.SendTextMessageAsync(MainHandler._colleaguegroup,
                            $"<b>🤝 درخواست  همکاری جدید :</b>\n\n" +
                            $"🔖 شناسه : #U{chatId}\n" +
                            $"👤 نام تلگرامی : <a href='tg://user?id={chatId}'>{user.FirstName + " " + user.LastName}</a>\n\n" +
                            $"✋🏻 {colleague.HowToSell.ToDisplay()}\n" +
                            $"🔗 {colleague.AverageOrder.ToDisplay()} فروش دارم.\n" +
                            $"👤 نام و نام خانوادگی : <b>{colleague.FullName}</b>\n" +
                            $"📞 شماره تماس : {colleague.PhoneNumber}\n"+
                            $"{colleague.HowMeetUs.ToDisplay()}\n\n" +
                            $"♻️ آیا اطلاعات فروشنده را تایید میکنید؟",
                            ParseMode.Html,
                            replyMarkup: SubscriberKeyboards.SellerConfirmation(chatId));
                        await _bot.SendTextMessageAsync(chatId,
                            $".\n" +
                            $"✳️ درخواست همکاری شما با موفقیت ارسال شد. \n" +
                            $"🔖 #U<code>{chatId}</code>\n\n" +
                            $"❗️ حساب کاربری شما پس از تائید مدیران ربات به حساب فروشنده تغییر خواهد یافت.\n\n" +
                            $"با سپاس از شکیبایی شما 🙏\n" +
                            $"تیم مدیریت {MainHandler.persianTitle}",
                            ParseMode.Html,
                            replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                        break;
                        break;
                    default:
                        break;
                }
                
            }
        }
        else if (data.StartsWith("settings*"))
        {
            var section = data.Replace("settings*", "");

            if (section.Equals("tocolleague"))
            {
                if (subscriber.Role == Role.Colleague)
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "شما هم اکنون جزو همکاران ما هستید.",
                        true);
                }
                else if (subscriber.Role == Role.Admin || subscriber.Role == Role.Owner)
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "مدیر نمیتواند فروشنده باشد.", true);
                }
                else
                {
                    await _bot.Choosed(callBackQuery);
                    await _bot.DeleteMessageAsync(chatId, callBackQuery.Message.MessageId);
                    await _bot.SendTextMessageAsync(chatId,
                        $".\n" +
                        $"تغییر حساب کاربری به فروشنده 🤝🏻\n\n" +
                        $"🔸 این قابلیت مختص همکاران فروش می باشد. دوستانی که قصد همکاری در فروش ما را داشته باشند، با تغییر حساب کاربری به حساب فروشنده از مزایای آن بهره مند خواهند شد.\n\n" +
                        $"▫️ برخی از مزایای حساب فروشنده :\n\n" +
                        $"🎖تعرفه های تعاملی (۴۰ الی ۵۰ درصد زیر قیمت)\n\n" +
                        $"🎖امکان خرید اعتباری\n\n" +
                        $"🎖دریافت تخفیف به تناسب تعداد خرید\n\n" +
                        $"🎖امکان تعیین نام دلخواه برای کانفیگ\n\n"+
                        $"🎖مدیریت و استعلام کانفیگ های خریداری شده\n\n" +
                        $"🎖درج تاریخ انقضا کانفیگ پس از اولین اتصال\n\n" +
                        $"🧩 جهت کسب اطلاعات بیشتر از شرایط و نحوه همکاری، تعرفه ها، نحوه تایید حساب فروشندگان و یا هر سوال دیگری در این خصوص، با آیدی زیر در ارتباط باشید :\n\n" +
                        $"🆔 {MainHandler.support}\n\n\n" +
                        $"🟡 توجه داشته باشید:\n\n" +
                        $"این قابلیت برای افردای که صرفا قصد خریداری سرویس های ما را برای اطرافیان خود دارند مناسب نمی‌باشد.پیشنهاد می‌شود از روش «کسب درآمد👥» استفاده کنید.\n\n" +
                        $"در صورت ارتقاء حساب کاربری به فروشنده، امکان برگشت وجود ندارد.آیا از ارتقاء حساب کاربری به فروشنده اطمینان دارید؟",
                        ParseMode.Html,
                        replyMarkup: SubscriberKeyboards.BecomeColleague());
                }
            }
            else if (section.Equals("back"))
            {
                await _bot.DeleteMessageAsync(chatId, callBackQuery.Message.MessageId);
            }
            else if (section.Equals("confirmcolleague"))
            {
                await _bot.DeleteMessageAsync(chatId, callBackQuery.Message.MessageId);

                Colleague? colleague = null;
                colleague = await _uw.ColleagueRepository.GetByChatId(subscriber.UserId);
                if (colleague is null)
                {
                    _uw.ColleagueRepository.Add(new Colleague()
                    {
                        UserId = subscriber.UserId,
                        JoinedOn = DateTime.Now,
                        Level = ColleagueLevel.Base,
                        Stage = ColleagueStage.FullName,
                    }); 
                    colleague = await _uw.ColleagueRepository.GetByChatId(subscriber.UserId);
                }

                switch (colleague.Stage)
                {
                    case ColleagueStage.FullName:
                        await _bot.Choosed(callBackQuery);

                        var msg = await _bot.SendTextMessageAsync(subscriber.UserId,
                            $"🔑 در حال تکمیل مراحل درخواست همکاری :\n" +
                            $"🔖 #U<code>{colleague.UserId}</code>\n" +
                            $"📌 مرحله اول از پنجم\n\n" +
                            $"✔️ نام و نام خانوادگی خود را ارسال کنید :",
                            ParseMode.Html,
                            replyMarkup: MarkupKeyboards.Cancel());
                         _uw.SubscriberRepository.ChangeStep(subscriber.UserId,
                            $"{Constants.SubscriberConstatns}-tocolleague*fullname*{msg.MessageId}");
                        break;
                    case ColleagueStage.PhoneNumber:
                        await _bot.Choosed(callBackQuery);
                        var msg_phone = await _bot.SendTextMessageAsync(subscriber.UserId,
                            $"🔑 در حال تکمیل مراحل درخواست همکاری :\n" +
                            $"🔖 #U<code>{colleague.UserId}</code>\n" +
                            $"📌 مرحله دوم از پنجم\n\n" +
                            $" - این مرحله اختباری می باشد\n" +
                            $"✔️ شماره تماس خود را از طریق دکمه زیر به اشتراک بگذارید :",
                            ParseMode.Html,
                            replyMarkup: MarkupKeyboards.ShareContact());
                        _uw.SubscriberRepository.ChangeStep(subscriber.UserId,
                            $"{Constants.SubscriberConstatns}-tocolleague*phonenumber*{msg_phone.MessageId}");
                        break;
                    case ColleagueStage.HowToSell:
                        await _bot.Choosed(callBackQuery);
                        await _bot.SendTextMessageAsync(subscriber.UserId,
                            $"🔑 در حال تکمیل مراحل درخواست همکاری :\n" +
                            $"🔖 #U<code>{colleague.UserId}</code>\n" +
                            $"📌 مرحله سوم از پنجم\n\n" +
                            $"✔️ فروش شما از چه طریق می باشد؟",
                            ParseMode.Html,
                            replyMarkup: SubscriberKeyboards.ColleagueHowToSell());
                        break;
                    case ColleagueStage.AverageOrder:
                        await _bot.Choosed(callBackQuery);

                        await _bot.SendTextMessageAsync(subscriber.UserId,
                            $"🔑 در حال تکمیل مراحل درخواست همکاری :\n" +
                            $"🔖 #U<code>{colleague.UserId}</code>\n" +
                            $"📌 مرحله چهارم از پنجم\n\n" +
                            $"✔️ میزان فروش کانفیگ شما بصورت میانگین چه تعداد در روز است؟",
                            ParseMode.Html,
                            replyMarkup: SubscriberKeyboards.ColleagueAverageOrder());
                        break;
                    case ColleagueStage.HowMeetUs:
                        await _bot.Choosed(callBackQuery);

                        await _bot.SendTextMessageAsync(subscriber.UserId,
                            $"🔑 در حال تکمیل مراحل درخواست همکاری :\n" +
                            $"🔖 #U<code>{colleague.UserId}</code>\n" +
                            $"📌 مرحله پنجم از پنجم\n\n" +
                            $"✔️ از چه طریق با وصل باش آشنا شده اید؟",
                            ParseMode.Html,
                            replyMarkup: SubscriberKeyboards.ColleagueHowToMeetUs());
                        break;
                    case ColleagueStage.Done:
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "درخواست همکاری شما در انتظار تایید می باشد.", true);
                        break;
                    default:
                        break;
                }
            }
            else if (section.Equals("notifications"))
            {
                await _bot.Choosed(callBackQuery);
                await _bot.SendTextMessageAsync(chatId,
                    $"✅️ فعالسازی این قابلیت، این امکان را به شما میدهد که ۴۸ ساعت مانده به اتمام سرویس خود، از ربات هشدار انقضای سرویس را دریافت و در صورت تمایل نسبت به تمدید سرویس خود اقدام نمائید\n\n" +
                    $"⚠️ توجه داشته باشید، پس از انقضای سرویس، امکان تمدید آن وجود نداشته و باید اکانت جدیدی از ربات خریداری نمائید.️",
                    ParseMode.Html,
                    replyMarkup: SubscriberKeyboards.NotificationSettings());
            }
            else if (section.Equals("updateremark"))
            {
                var colleague = await _uw.ColleagueRepository.GetByChatId(chatId);
                if (!colleague.Tag.HasValue())
                {
                    await _bot.SendTextMessageAsync(chatId,
                        $"☢ لطفا remark کانفیگ های خود را تعیین نمائید. در حالت پیش فرض، کانفیگ ها با ریمارک {MainHandler.remark} تولید می شوند.\n\n❓ریمارک، نام انتهایی کانفیگ های شماست که نشانگر برند شما به مشتری می باشد.",
                        replyMarkup: InlineKeyboards.AboutRemark());
                    await _bot.SendTextMessageAsync(chatId, $"ریمارک خود را ارسال نمایید :");
                    _uw.SubscriberRepository.ChangeStep(chatId, "sendremark");
                }
                else
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "شما قبلا ریمارک خود را ثبت کرده اید.",
                        true);
                }
            }
        }
        else if (data.Equals("aboutremark"))
        {
            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "انتخاب شد.");
            await _bot.DeleteMessageAsync(chatId, callBackQuery.Message.MessageId);
            await _bot.SendTextMessageAsync(chatId,
                $"☢ لطفا remark کانفیگ های خود را تعیین نمائید. در حالت پیش فرض، کانفیگ ها با ریمارک {MainHandler.remark} تولید می شوند.\n\n❓ریمارک، نام انتهایی کانفیگ های شماست که نشانگر برند شما به مشتری می باشد.\n\n❇️ به عنوان مثال کانفیگ زیر را در نظر بگیرید:\n\n\nvless://7c14bddf-d058-4ac5-91d7-b658ea97888b@test.{MainHandler.title}.co:80?path=%2f&security=none&encryption=none&type=ws#{MainHandler.remark}1m1c324\n\n🔺 در کانفیگ بالا، {MainHandler.remark} ریمارک مجموعه {MainHandler.persianTitle} می باشد، شما می توانید از نام فروشگاه یا هر نام انتخابی دیگری برای تولید کانفیگ های خود استفاده کنید.\n\n🔻 چنانچه نام برند شما mobiletest باشد، کانفیگ های تولید شده به صورت زیر خواهد بود:\n\nvless://7c14bddf-d058-4ac5-91d7-b658ea97888b@test.{MainHandler.title}.co:80?path=%2f&security=none&encryption=none&type=ws#mobiletest1m1c324\n\n⚠️ توجه داشته باشید که 1m1c و عدد انتهایی که در مثال بالا 324 می باشد، جهت شناسایی کانفیگ شما در سرور مورد استفاده قرار میگیرد و توصیه می شود جهت پیگیری مشکلات احتمالی از حذف آن خودداری نمائید.",
                replyMarkup: MarkupKeyboards.Cancel());
        }
        else if (data.StartsWith("notifications*"))
        {
            var state = data.Replace("notifications*", "") == "turnon" ? true : false;
            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                $"وضعیت اعلانات به {(state ? "فعال" : "غیرفعال")} تغییر یافت.", true);
            subscriber.Notification = state;
            _uw.SubscriberRepository.Update(subscriber);

            await _bot.DeleteMessageAsync(chatId, callBackQuery.Message.MessageId);
        }
        else if (data.StartsWith("upgradelevel*"))
        {
            var colleague =
                await _uw.ColleagueRepository.GetByChatId(long.Parse(data.Replace("upgradelevel*", "")));
            if (colleague is not null)
                switch (colleague.Level)
                {
                    case ColleagueLevel.Base:
                        colleague.Level = ColleagueLevel.Bronze;
                        _uw.ColleagueRepository.Update(colleague);
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "همکار به سطح برنزی ارتقا یافت.",
                            true);
                        break;
                    case ColleagueLevel.Bronze:
                        colleague.Level = ColleagueLevel.Silver;
                        _uw.ColleagueRepository.Update(colleague);
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "همکار به سطح نقره ای ارتقا یافت.",
                            true);
                        break;
                    case ColleagueLevel.Silver:
                        colleague.Level = ColleagueLevel.Gold;
                        _uw.ColleagueRepository.Update(colleague);
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "همکار به سطح طلایی ارتقا یافت.",
                            true);
                        break;
                    case ColleagueLevel.Gold:
                        colleague.Level = ColleagueLevel.Pro;
                        _uw.ColleagueRepository.Update(colleague);
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "همکار به سطح حرفه ای ارتقا یافت.",
                            true);
                        break;
                    case ColleagueLevel.Pro:
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                            "همکار در آخرین سطح کاربری قرار دارد.", true);
                        break;
                    default:
                        break;
                }
        }
        else if (data.StartsWith("checkseller*"))
        {
            var userId = long
                .Parse(data.Split("*")[1]);
            var answer = data.Split("*")[2];
            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, $"انتخاب شد.");

            if (answer == "approve")
            {
                var collegue = await _uw.ColleagueRepository.GetByChatId(userId);
                if (collegue is not null)
                {
                    var u = await _uw.SubscriberRepository.GetByChatId(collegue.UserId);
                    u.Role = Role.Colleague;
                    _uw.SubscriberRepository.Update(u);
                    await _bot.EditMessageTextAsync(groupId, callBackQuery.Message.MessageId,
                        callBackQuery.Message.Text.Replace("♻️ آیا اطلاعات فروشنده را تایید میکنید؟",
                            $"توسط {user.FirstName + " " + user.LastName} با موفقیت تایید شد.✅"));
                 
                    await _bot.SendTextMessageAsync(userId, $"🎉 تبریک!\n\n" +
                                                            $"✅ درخواست تغییر حساب کاربری شما به حساب فروشنده به شناسه #U{userId} با موفقیت تائید شد.\n\n" +
                                                            $"با فرستادن دستور /start میتوانین از امکانات حساب فروشندگان استفاده نمائید.");
                }
                else
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "همکار هم اکنون وجود دارد.", true);
                }
            }
            else
            {
                await _bot.EditMessageTextAsync(groupId, callBackQuery.Message.MessageId,
                    callBackQuery.Message.Text.Replace("♻️ آیا اطلاعات فروشنده را تایید میکنید؟",
                        $"توسط {user.FirstName + " " + user.LastName} با موفقیت رد شد.✖️"));
                await _bot.SendTextMessageAsync(userId, $"درخواست تبدیل حساب شما رد شد.\n" +
                                                        $"جهت پیگیری علت به پشتیبانی مراجعه نمایید.✖️");
            }
        }
        else if (data.Equals("sendtoall"))
        {
            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "انتخاب شد.");
            await _bot.SendTextMessageAsync(groupId,
                $"📩 قصد ارسال پیام همگانی به کدام بخش از کاربران را دارید؟",
                replyMarkup: SubscriberKeyboards.SendToAllContactTypes());
        }
        else if (data.Equals("management"))
        {
            await _bot.Choosed(callBackQuery);

            var activeUsers = await _uw.AccountRepository.GetActiveOnes();
            var gottestusers = await _uw.AccountRepository.GotTestUsers();

            var countsAll = _uw.SubscriberRepository.GetCount();
            var admins = await _uw.SubscriberRepository.GetAdmins();

            var sellers = await _uw.SubscriberRepository.GetSellers();

            await _bot.SendTextMessageAsync(groupId,
                $"مدیریت کاربران سیستم :\n\n" +
                $"🤖 تعداد کاربران ربات : <b>{countsAll.ToString().En2Fa()} نفر</b>\n" +
                $"✔️ تعداد کاربران فعال : <b>{activeUsers.Count.ToString().En2Fa()} نفر</b>\n" +
                $"🧪️ تعداد کاربران دریافت تست : <b>{gottestusers.Count.ToString().En2Fa()} نفر</b>\n" +
                $"♻️ تعداد فروشندگان : <b>{sellers.ToString().En2Fa()} نفر</b>\n" +
                $"🧑‍💻 تعداد مدیران : <b>{admins.Count.ToString().En2Fa()} نفر</b>\n\n"
                , ParseMode.Html);
        }
        else if (data.StartsWith("tickettotuser*"))
        {
            var u = await _uw.SubscriberRepository.GetByChatId(
                long.Parse(data.Replace("tickettotuser*", "")));
            if (u is not null)
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                    "یپام خود را به کاربر از طریق ربات ارسال کنید.", true);
                await _bot.SendTextMessageAsync(chatId,
                    $".\n" +
                    $"💬 شما در حال ارسال پیام به کاربر هستید.\n\n" +
                    $"👤 <code>#U{u.UserId}</code> | <a href='tg://user?id={u.UserId}'>{u.FullName}</a>\n\n" +
                    $"پیام خود را ارسال کنید :",
                    ParseMode.Html,
                    replyMarkup: MarkupKeyboards.Cancel());

                _uw.SubscriberRepository.ChangeStep(chatId, $"{Constants.SubscriberConstatns}-sendticket*{u.UserId}");
            }
        }
        else if (data.StartsWith("getreport*"))
        {
            var type = data.Split("*")[1];
            if (type.Equals("transactions"))
            {
                var transactions =
                    await _uw.TransactionRepository.GetAllMineAsync(long.Parse(data.Split("*")[2]));
                if (transactions.Count != 0)
                {
                    _uw.ExcelService.TransactionsToCsv(transactions, _bot, groupId);
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                        "گزارش تراکنش های کاربر با موفقیت ارسال شد.", true);
                }
                else
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "کاربر تراکنش ثبت شده ای ندارد.",
                        true);
                }
            }
            else if (type.Equals("subscribers"))
            {
                var subscribers = _uw.SubscriberRepository.GetAll();
                var report_models = new List<SubscriberReportModel>();

                var msg = await _bot.SendTextMessageAsync(groupId, $"📂 در حال گزارش گیری لیست کاربران..");
                var counter = 0;
                foreach (var sub in subscribers)
                {
                    var orders = await _uw.OrderRepository.GetMineCountAsync(sub.UserId);
                    var accounts = await _uw.AccountRepository.GetMineCountAsync(sub.UserId);
                    var balance = await _uw.TransactionRepository.GetMineBalanceAsync(sub.UserId);

                    report_models.Add(new SubscriberReportModel()
                    {
                        UserId = sub.UserId,
                        Accounts = accounts != null ? accounts.Value : 0,
                        Orders = orders != null ? orders.Value : 0,
                        Balance = balance.Value.ToIranCurrency(),
                        JoinedOn = sub.JoinedOn.ToPersianDate(),
                        isActive = sub.IsActive ? "فعال" : "غیرفعال",
                        Username = sub.Username,
                        Role = sub.Role.ToDisplay(),
                        FullName = sub.FullName
                    });
                    counter++;
                    if (counter % 50 == 0)
                        await _bot.EditMessageTextAsync(groupId, msg.MessageId,
                            $"تا کتون از <b>{counter.En2Fa()} از {subscribers.Count().En2Fa()}</b> کاربر گزارش دریافت شده..."
                            , ParseMode.Html);
                }

                _uw.ExcelService.SubscribersToCsv(report_models, _bot, groupId, msg.MessageId);
            }
            else if (type.Equals("colleagues"))
            {
                var colleagues = _uw.ColleagueRepository.GetAll();
                var report_models = new List<ColleagueReportModel>();

                var msg = await _bot.SendTextMessageAsync(groupId, $"📂 در حال گزارش گیری لیست همکاران..");
                foreach (var colleague in colleagues)
                {
                    var orders = await _uw.OrderRepository.GetMineOrders(colleague.UserId);
                    var accounts = await _uw.AccountRepository.GetMineAccountsAsync(colleague.UserId);
                    var balance = await _uw.TransactionRepository.GetMineBalanceAsync(colleague.UserId);

                    report_models.Add(new ColleagueReportModel()
                    {
                        UserId = colleague.UserId,
                        Accounts = accounts.Count,
                        Orders = orders.Count,
                        TotalPayments = (long)orders.Sum(s => s.TotalAmount),
                        Tag = colleague.Tag,
                        Balance = balance.Value,
                        JoinedOn = colleague.JoinedOn.ToPersianDate()
                    });
                }

                _uw.ExcelService.ColleaguesToCsv(report_models, _bot, groupId, msg.MessageId);
            }
        }
        else if (data.StartsWith("changerole*"))
        {
            var role = data.Split("*")[1];
            var u = await _uw.SubscriberRepository.GetByChatId(long.Parse(data.Split("*")[2]));

            if (role == "subscriber")
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                    "فروشنده با موفقیت به نقش کاربر تنزل یافت.✅",
                    true);
                u.Role = Role.Subscriber;
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                    "کاربر با موفقیت به نقش کاربر ارتقا یافت.✅",
                    true);
                u.Role = Role.Colleague;
            }

            _uw.SubscriberRepository.Update(u);
            await _bot.DeleteMessageAsync(groupId, callBackQuery.Message.MessageId);

            var orders = await _uw.OrderRepository.GetMineOrders(u.UserId);
            var accounts = await _uw.AccountRepository.GetMineAccountsAsync(u.UserId);

            var activeAcc = accounts.Where(s => s.IsActive && s.EndsOn > DateTime.Now).ToList();
            var checkAcc = accounts.FirstOrDefault(s => s.Type == AccountType.Check);

            var totalPayments = 0m;
            if (orders.Count > 0) totalPayments = orders.Sum(s => s.Amount);

            Colleague colleague = null;
            if (u.Role.Equals(Role.Colleague)) colleague = await _uw.ColleagueRepository.GetByChatId(u.UserId);
            await _bot.SendTextMessageAsync(groupId,
                $"👤 اطلاعات کاربری : \n\n" +
                $"🔖 شناسه کاربری :  #U{u.UserId}\n" +
                $"✏️ نام و نام خانوادگی : <b>{u.FullName}</b>\n" +
                $"{(u.Username.HasValue() ? $"✔️ نام کاربری : <b>@{u.Username}</b>\n" : "")}" +
                $"🕚 تاریخ عضویت :\n" +
                $"<b>{u.JoinedOn.ConvertToPersianCalendar().En2Fa()}</b>\n" +
                $"💂‍♀️ نقش کاربر : <b>{u.Role.ToDisplay()}</b> \n\n" +
                $"💳 مجموع پرداختی تا کنون : <b>{totalPayments.ToIranCurrency().En2Fa()} تومان</b>\n" +
                $"🛒 تعداد سفارشات تا کنون : <b>{orders.Count.ToString().En2Fa()}</b>\n" +
                $"🔗 تعداد اشتراک های فعال : <b>{activeAcc.Count.ToString().En2Fa()}</b>\n" +
                $"📌 اکانت تست : <b>{(checkAcc is null ? "دریافت نکرده است." : "دریافت کرده است.")}</b>",
                ParseMode.Html,
                replyMarkup:
                SubscriberKeyboards.SingleUserManagement(u, colleague));
        }
        else if (data.StartsWith("useractivation*"))
        {
            var activate_to = data.Split("*")[1];
            var u = await _uw.SubscriberRepository.GetByChatId(long.Parse(data.Split("*")[2]));
            if (u is not null)
            {
                if (activate_to.Equals("ban"))
                {
                    u.IsActive = false;
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "کاربر مورد نظر با موفقیت بن شد.✅",
                        true);
                }
                else
                {
                    u.IsActive = true;
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "کاربر مورد نظر با موفقیت آنبن شد.✅",
                        true);
                }

                _uw.SubscriberRepository.Update(u);

                await _bot.DeleteMessageAsync(groupId, callBackQuery.Message.MessageId);
                Colleague colleague = null;
                if (u.Role.Equals(Role.Colleague))
                    colleague = await _uw.ColleagueRepository.GetByChatId(u.UserId);
                var orders = await _uw.OrderRepository.GetMineOrders(u.UserId);
                var accounts = await _uw.AccountRepository.GetMineAccountsAsync(u.UserId);

                var activeAcc = accounts.Where(s => s.IsActive && s.EndsOn > DateTime.Now).ToList();
                var checkAcc = accounts.FirstOrDefault(s => s.Type == AccountType.Check);

                var totalPayments = 0m;
                if (orders.Count > 0) totalPayments = orders.Sum(s => s.Amount);
                await _bot.SendTextMessageAsync(groupId,
                    $"👤 اطلاعات کاربری : \n\n" +
                    $"🔖 شناسه کاربری :  #U{u.UserId}\n" +
                    $"✏️ نام و نام خانوادگی : <b>{u.FullName}</b>\n" +
                    $"{(u.Username.HasValue() ? $"✔️ نام کاربری : <b>@{u.Username}</b>\n" : "")}" +
                    $"🕚 تاریخ عضویت :\n" +
                    $"<b>{u.JoinedOn.ConvertToPersianCalendar().En2Fa()}</b>\n" +
                    $"💂‍♀️ نقش کاربر : <b>{u.Role.ToDisplay()}</b> \n\n" +
                    $"💳 مجموع پرداختی تا کنون : <b>{totalPayments.ToIranCurrency().En2Fa()} تومان</b>\n" +
                    $"🛒 تعداد سفارشات تا کنون : <b>{orders.Count.ToString().En2Fa()}</b>\n" +
                    $"🔗 تعداد اشتراک های فعال : <b>{activeAcc.Count.ToString().En2Fa()}</b>\n" +
                    $"📌 اکانت تست : <b>{(checkAcc is null ? "دریافت نکرده است." : "دریافت کرده است.")}</b>",
                    ParseMode.Html,
                    replyMarkup:
                    SubscriberKeyboards.SingleUserManagement(u, colleague));
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "کاربر مورد نظر یافت نشد.", true);
            }
        }
        else if (data.StartsWith("contacts*"))
        {
            var contacts = data.Replace("contacts*", "");
            var type = "تمام کاربران";
            if (contacts.Equals("subscribers"))
                type = "مشتریان";
            if (contacts.Equals("factors"))
                type = "فاکتور کننده های بدون پرداخت";
            else if (contacts.Equals("sellers"))
                type = "فروشندگان";
            else if (contacts.Equals("subscribers"))
                type = "";
            else if (contacts.Equals("notgettest"))
                type = "کاربران تست دریافت نکرده";
            else if (contacts.Equals("notusetoffer"))
                type = "کاربران تخفیف استفاده نکرده";
            else if (contacts.Equals("activeconfigs"))
                type = "کاربران دارای کانفیگ فعال";


            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                "هم اکنون می توانید از طریق پی وی برای ارسال پیام همگانی اقدام نمایید.", true);
            await _bot.SendTextMessageAsync(chatId,
                $"لطفا متن پیام خود را جهت ارسال به <b>{type}</b> ارسال نمایید :",
                ParseMode.Html,
                replyMarkup: MarkupKeyboards.Cancel());

            _uw.SubscriberRepository.ChangeStep(chatId, $"{Constants.SubscriberConstatns}-sendtoall*{data.Replace("contacts*", "")}");
        }

    }
}