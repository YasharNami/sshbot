using System.Text.RegularExpressions;
using ConnectBashBot.Commons;
using SSHVpnBot.Components;
using SSHVpnBot.Components.AccountReports.Handlers;
using SSHVpnBot.Components.Accounts;
using SSHVpnBot.Components.Accounts.Handlers;
using SSHVpnBot.Components.Accounts.Keyboards;
using SSHVpnBot.Components.Checkouts;
using SSHVpnBot.Components.Colleagues;
using SSHVpnBot.Components.Discounts.Handlers;
using SSHVpnBot.Components.Locations;
using SSHVpnBot.Components.Orders;
using SSHVpnBot.Components.Orders.Handlers;
using SSHVpnBot.Components.Orders.Keyboards;
using SSHVpnBot.Components.Servers.Handlers;
using SSHVpnBot.Components.ServiceCategories.Handlers;
using SSHVpnBot.Components.ServiceCategories.Repository;
using SSHVpnBot.Components.Services;
using SSHVpnBot.Components.Services.Handlers;
using SSHVpnBot.Components.Subscribers;
using SSHVpnBot.Components.Subscribers.Handlers;
using SSHVpnBot.Components.Subscribers.Keyboards;
using SSHVpnBot.Components.Transactions;
using SSHVpnBot.Repositories.Uw;
using SSHVpnBot.Services.Panel.Models;
using SSHVpnBot.Telegram;
using SSHVpnBot.Telegram.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using File = System.IO.File;

namespace ConnectBashBot.Telegram.Handlers;

public static class MessageHandler
{
    public static async Task HandlePrivateMessageAsync(this ITelegramBotClient _bot, Update update, IUnitOfWork _uw,
        CancellationToken cancellationToken = default)
    {
        var messageType = update.Message!.Type;
        var message = update.Message;
        var user = update.Message.From!;
        var chatId = user.Id;

        var step = await _uw.SubscriberRepository.CheckStep(chatId);

        if (step == null)
        {
            var new_suscriber = await user.CreateSubscriberAsync();
            _uw.SubscriberRepository.AddWithId(new_suscriber);
            step = new_suscriber.Step;
        }

        await _uw.LoggerService.LogMessage(Program.logger_bot, update, step);


        var userInfo = await _uw.SubscriberRepository.GetByChatId(chatId);


        var joined = await _bot.CheckJoin(chatId, MainHandler._mainchannel);
        if (!joined)
        {
            await _bot.SendTextMessageAsync(chatId,
                $"🔗 پیش از شروع کار با ربات در کانال ما عضو شوید :",
                replyMarkup: InlineKeyboards.Joined());
            return;
        }

        if (userInfo.IsActive)
        {
            if (messageType == MessageType.Text)
            {
                if (message.Text.StartsWith("/start"))
                {
                    if (update.Message.Text.Contains("ref="))
                    {
                        var referral = update.Message.Text.Split("ref=")[1];
                        if (!userInfo.Referral.HasValue())
                            if (long.Parse(referral) != chatId)
                            {
                                userInfo.Referral = referral;
                                _uw.SubscriberRepository.Update(userInfo);
                                var code = Transaction.GenerateNewDiscountNumber();
                                var transaction = new Transaction()
                                {
                                    Amount = 5000,
                                    CreatedOn = DateTime.Now,
                                    TransactionCode = code,
                                    IsRemoved = false,
                                    UserId = chatId,
                                    Type = TransactionType.StartViaReferral
                                };
                                _uw.TransactionRepository.Add(transaction);

                                await _bot.SendTextMessageAsync(chatId,
                                    $".\n" +
                                    $"به ربات {MainHandler.persianTitle} خوش آمدید.🎉\n\n" +
                                    $"💰 مبلغ <b>{transaction.Amount.ToIranCurrency().En2Fa()} تومان</b> به کیف پول شما اضافه شد.\n" +
                                    $"از قسمت '👤 حساب کاربری'\n" +
                                    $"می توانید موجودی خود را مشاهده کنید.\n" +
                                    $".",
                                    ParseMode.Html, replyMarkup: MarkupKeyboards.Main(userInfo.Role));
                            }
                    }

                    await _bot.SendTextMessageAsync(chatId,
                        "🔹 لطفا گزینه مورد نظر را از منو انتخاب نمائید",
                        replyMarkup: MarkupKeyboards.Main(userInfo.Role));

                    _uw.SubscriberRepository.ChangeStep(chatId, "none");
                }
                else if (message.Text.StartsWith("/member"))
                {
                    int? counts = 0;
                    if (message.Text.Equals("/member"))
                        counts = _uw.SubscriberRepository.GetCount();
                    else
                        counts = await _uw.SubscriberRepository.GetAllByReferral(message.Text.Replace("/member_", "")
                            .ToString());

                    await _bot.SendTextMessageAsync(chatId,
                        $"<b>تعداد افراد عضو شده :</b>", parseMode: ParseMode.Html,
                        replyMarkup: SubscriberKeyboards.Members(counts.Value));
                }
                else if (message.Text.Equals("🗣 کسب درآمد از زیرمجموعه گیری") || message.Text.Equals("🗣 کسب درآمد"))
                {
                    var referrals = await _uw.SubscriberRepository.GetAllByReferral(chatId.ToString());
                    var balance = await _uw.TransactionRepository.GetMineReferralBalance(chatId);

                    Message banner_msg = null;
                    using (var fs = new MemoryStream(File.ReadAllBytes("./medias/cb.jpg")))
                    {
                        banner_msg = await _bot.SendPhotoAsync(chatId, new InputOnlineFile(fs, "connectbash"),
                            caption: $"سلام👋\n\n" +
                                     $"گوشیت پره فیلترشکن و پروکسیه و هچیکدومشونم کار نمیکنه؟؟😢\n\n" +
                                     $"🔗 اگه در اتصال به اینستاگرام و تلگرام با مشکل روبرو هستی و دنبال یک vpn قوی و پرسرعت اختصاصی میگردی، سریع این ربات رو استارت کن و رو دکمه <b>دریافت تست رایگان</b> بزن و وصل شو تا سرعت نور رو تجربه کنی🚀\n\n" +
                                     $"http://t.me/connectbashbot?start=ref={chatId}\n" +
                                     $".",
                            ParseMode.Html);
                    }


                    await _bot.SendTextMessageAsync(chatId,
                        $".\n\n" +
                        $"بنر شما با موفقیت ساخته شد.✅\n\n" +
                        $"<b>با دعوت کردن دوستات به {MainHandler.persianTitle}<b>۱۰ درصد از مبلغ خرید دوستات</b> به کیف پولت اضافه میشه.😉</b>\n" +
                        $"و همینطور زیرمجموعه شمابا عضویت از طریق لینک شما <b>۵۰۰۰ تومان</b> اعتبار دریافت میکنه. \n\n" +
                        $"لینک دعوت شما 👇🏻:\n" +
                        $"🔗 <code>http://t.me/connectbashbot?start=ref={chatId}</code>\n\n" +
                        $"💰 موجودی فعلی شما از زیرمجموعه ها :\n" +
                        $"<b>{((decimal)balance.Value).ToIranCurrency().En2Fa()} تومان</b>\n" +
                        $"👥 تعداد زیر مجموعه ها : <b>{referrals.Value.En2Fa()} نفر</b>",
                        ParseMode.Html,
                        disableWebPagePreview: true,
                        replyToMessageId: banner_msg?.MessageId,
                        replyMarkup: InlineKeyboards.CheckoutReferral());
                }
                else if (message.Text.Trim().Fa2En().ToLower().Equals("/charge"))
                {
                    var u = await _uw.SubscriberRepository.GetByChatId(chatId);
                    if (u.JoinedOn < DateTime.Parse("8/20/2023 12:45:49 PM"))
                    {
                        var used = await _uw.TransactionRepository.AnyByTypeAndUserId(u.UserId,
                            TransactionType.Apology);
                        if (!used)
                        {
                            var transaction = new Transaction()
                            {
                                Type = TransactionType.Apology,
                                Amount = 50000,
                                IsRemoved = false,
                                UserId = u.UserId,
                                CreatedOn = DateTime.Now,
                                TransactionCode = Transaction.GenerateNewDiscountNumber()
                            };
                            _uw.TransactionRepository.Add(transaction);
                            await _bot.SendTextMessageAsync(chatId,
                                $"حساب کاربری شما به مبلغ <b>۵۰،۰۰۰ تومان</b> شارژ شد.✔️\n",
                                ParseMode.Html);
                            var balance = await _uw.TransactionRepository.GetMineBalanceAsync(u.UserId);
                            await _bot.SendTextMessageAsync(MainHandler._managementgroup,
                                $".\n" +
                                $"🎁 #آفرـاستفاده_شد.\n\n" +
                                $"👤 توسط <a href='tg://user?id={chatId}'>{user.FirstName + " " + user.LastName}</a> | <b>#U{u.UserId}</b>\n\n" +
                                $"💰 به مبلغ <b>{transaction.Amount.ToIranCurrency().En2Fa()} تومان</b>\n" +
                                $"🧢 موجودی کاربر : <b>{balance.Value.ToIranCurrency().En2Fa()} تومان</b>\n" +
                                $"🕙 در تاریخ <b>{transaction.CreatedOn.ConvertToPersianCalendar().En2Fa()}</b>\n" +
                                $".",
                                ParseMode.Html);
                        }
                        else
                        {
                            await _bot.SendTextMessageAsync(chatId,
                                $"این آفر قبلا توسط شما استفاده شده است. ✖️",
                                ParseMode.Html);
                        }
                    }
                    else
                    {
                        await _bot.SendTextMessageAsync(chatId,
                            $"این آفر به شما تعلق ندارد. ✖️",
                            ParseMode.Html);
                    }
                }
                else if (message.Text.Equals("🔗 خرید اشتراک جدید") || message.Text.Equals("🚀 خرید اکانت جدید") ||
                         message.Text.Equals("وی پی ان اختصاصی 🔗"))
                {
                   
                        var services = _uw.ServiceRepository.GetAll()
                            .Where(s => s.IsActive && !s.IsRemoved).ToList();
                        await _bot.SendTextMessageAsync(user.Id, 
                            ".\n" +
                            "🔗 اشتراک مورد نظر خود را انتخاب کنید :", ParseMode.Html,
                            replyMarkup: await OrderKeyboards.Services(_uw, userInfo, services));
                }
                else if (message.Text.Equals("📁 سفارشات من"))
                {
                    var orders = await _uw.OrderRepository.GetMineOrders(user.Id);
                    var accounts = (await _uw.AccountRepository.GetMineAccountsAsync(userInfo.UserId)).Where(s =>
                        s.Type != AccountType.Check).ToList();
                    var actives = accounts.Where(s => s.State.Equals(AccountState.Active) && s.EndsOn > DateTime.Now)
                        .Count();
                    var colleague = await _uw.ColleagueRepository.GetByChatId(user.Id);
                    if (orders.Count > 0)
                        await _bot.SendTextMessageAsync(user.Id,
                            $".\n" +
                            $"<b>📁 مدیریت سفارشات موفق</b>\n\n" +
                            $"🔖 تعداد سفارشات انجام شده : <b>{orders.Count.En2Fa()} عدد</b>\n" +
                            $"🔗 مجموع اشتراک دریافتی : <b>{orders.Sum(s => s.Count).En2Fa()} عدد</b>\n" +
                            $"🟢 تعداد اشتراک فعال : <b>{actives.En2Fa()} عدد</b>\n" +
                            $"💳 مجموع پرداختی : <b>{orders.Sum(s => s.TotalAmount).ToIranCurrency().En2Fa()} تومان</b>\n\n" +
                            $"🗂 صفحه <b>{1.En2Fa()}</b> از <b>{Math.Ceiling((decimal)accounts.Count() / 20).En2Fa()}</b>\n\n" +
                            $"📌 قصد مشاهده اطلاعات کدام اشتراک را دارید؟",
                            ParseMode.Html,
                            replyMarkup: AccountKeyboards.MineAccounts(accounts, colleague, 1));
                    else
                        await _bot.SendTextMessageAsync(user.Id, $"سفارشی یافت نشد.🤔",
                            replyToMessageId: message.MessageId);
                }
                else if (message.Text.Equals("انصراف ✖️"))
                {
                    await _bot.SendTextMessageAsync(chatId,
                        "🔹 لطفا گزینه مورد نظر را از منو انتخاب نمائید",
                        replyMarkup: MarkupKeyboards.Main(userInfo.Role));
                    _uw.SubscriberRepository.ChangeStep(chatId, "none");
                    return;
                }
                else if (message.Text.Equals("🎯 اشتراک های من"))
                {
                    var mine = await _uw.AccountRepository.GetMineAccountsAsync(chatId);
                    if (mine.Count != 0)
                    {
                        if (userInfo.Role != Role.Colleague)
                        {
                            var serices = new List<Service>();
                            foreach (var account in mine.Where(a =>
                                             a.Type == AccountType.Normal && a.IsActive && a.ServiceCode != "BYADMIN")
                                         .ToList())
                            {
                                var service = await _uw.ServiceRepository.GetServiceByCode(account.ServiceCode);
                                serices.Add(service);
                            }

                            await _bot.SendTextMessageAsync(chatId,
                                $".\n\n" +
                                $"🔹 اشتراک خود را جهت بررسی انتخاب نمایید :",
                                ParseMode.Html,
                                replyMarkup: AccountKeyboards.MineServices(mine, serices));
                        }
                    }
                    else
                    {
                        await _bot.SendTextMessageAsync(chatId, "سرویس فعالی ندارد.✖️");
                    }
                }
                else if (message.Text.Equals("💬 پشتیبانی"))
                {
                    await _bot.SendTextMessageAsync(chatId,
                        "📚 در چه خصوصی نیاز به پشتیبانی دارید؟", replyMarkup: InlineKeyboards.Help());
                }
                else if (message.Text.Equals("👤 حساب کاربری"))
                {
                    var balance = await _uw.TransactionRepository.GetMineBalanceAsync(chatId);
                    var accounts = (await _uw.AccountRepository.GetMineAccountsAsync(chatId))
                        .Where(s => s.State == AccountState.Active).ToList();
                    var orders = await _uw.OrderRepository.GetMineOrders(chatId);
                    Colleague? colleague = null;
                    await _bot
                        .SendTextMessageAsync(chatId,
                            $".\n" +
                            $"👤 شناسه کاربر :\n" +
                            $"<code>#U{userInfo.UserId}</code>\n\n" +
                            $"💂‍♀ نقش : <b>{userInfo.Role.ToDisplay()}</b>\n" +
                            $"{(colleague is not null ? $"🌀 سطح همکاری : <b>{colleague.Level.ToDisplay()}</b>\n" : "")}" +
                            $"🕑 تاریخ عضویت :\n" +
                            $"<b>{userInfo.JoinedOn.ConvertToPersianCalendar().En2Fa()}</b>\n" +
                            $"💰 موجودی : <b>{balance.Value.ToIranCurrency().En2Fa()} تومان</b>\n\n" +
                            $"📌 تاریخ آخرین سفارش : \n" +
                            $"<b>{(orders.Count == 0 ? "-" : orders.OrderByDescending(s => s.CreatedOn).LastOrDefault().CreatedOn.ConvertToPersianCalendar().En2Fa())}</b>\n" +
                            $"📌 تعداد سفارشات تاکنون : <b>{orders.Count.En2Fa()}</b>\n" +
                            $"📌 تعداد اشتراک های فعال : <b>{(accounts.Count > 0 ? accounts.Count.En2Fa() : 0)}</b>\n" +
                            $"📌 مقدار پرداختی تاکنون : <b>{orders.Sum(s => s.TotalAmount).ToIranCurrency().En2Fa()} تومان</b>\n" +
                            $".",
                            ParseMode.Html,
                            replyMarkup: SubscriberKeyboards.Profile(userInfo));
                }
                else if (message.Text.Equals("💰 شارژ کیف پول"))
                {
                    var blanace = await _uw.TransactionRepository.GetMineBalanceAsync(chatId);

                    await _bot.SendTextMessageAsync(chatId,
                        $".\n" +
                        $"💰 موجودی شما : <b>{blanace.Value.ToIranCurrency().En2Fa()} تومان</b>\n\n" +
                        $"<b>📥 مقدار شارژ تومانی خود را انتخاب یا وارد کنید :</b>",
                        ParseMode.Html,
                        replyMarkup: SubscriberKeyboards.ChargeAmounts());
                    _uw.SubscriberRepository.ChangeStep(chatId, $"{Constants.SubscriberConstatns}-chargewallet");
                }
                else if (message.Text.Equals("دریافت پنل اختصاصی ⚜️"))
                {
                    await _bot.SendTextMessageAsync(chatId,
                        "🌀 از این قسمت امکان دریافت نمایندگی فروش برای شما فراهم خواهد شد.\n\n" +
                        "🔻 جهت مشاهده امکانات سیستم نوع نمایندگی خود را انتخاب نمایید :", parseMode: ParseMode.Html
                        , replyMarkup: InlineKeyboards.Representations(), cancellationToken: cancellationToken);
                }
                else if (message.Text.Equals("💰 کیف پول"))
                {
                    var balance = await _uw.TransactionRepository.GetMineBalanceAsync(chatId);
                    var balabce_amount = balance.Value;

                    await _bot.SendTextMessageAsync(chatId,
                        $"💰 موجودی شما : <b>{((decimal)balabce_amount).ToIranCurrency().En2Fa()} تومان</b>\n\n" +
                        $"🔻 این موجودی قابل برداشت نمی باشد و صرفا جهت خرید اعتباری شما از سیستم توسط مدیریت قابل شارژ است.\n" +
                        $"🔻 جهت شارژ حساب خود با پشتیبانی در ارتباط باشید.",
                        ParseMode.Html, cancellationToken: cancellationToken);
                }
                else if (message.Text.Equals("🔎 بررسی سرویس") || message.Text.Equals("🔎 استعلام اشتراک"))
                {
                    var msg = await _bot.SendTextMessageAsync(chatId,
                        "🔍", replyMarkup: MarkupKeyboards.Cancel(), cancellationToken: cancellationToken);
                    await _bot.DeleteMessageAsync(chatId, msg.MessageId, cancellationToken);
                    await _bot.SendTextMessageAsync(chatId,
                        $".\n" +
                        $"🔍 شناسه کانفیگ مشتری خود را جهت بررسی وارد نمایید :\n\n" +
                        $"به عنوان مثال :\n" +
                        $"🔖 88863817-cc4d-4f80-94d0-1e7bb0c6a7c7",
                        ParseMode.Html,
                        replyMarkup: AccountKeyboards.SearchAccount(), cancellationToken: cancellationToken);

                    _uw.SubscriberRepository.ChangeStep(chatId, $"{Constants.AccountConstants}-senduid");
                }
                else if (message.Text.Equals("🧪 دریافت اکانت تست"))
                {
                   var msg = await _bot.SendTextMessageAsync(chatId, $"در حال ساخت اکانت تست...");
                   var mine = await _uw.AccountRepository.GetMineAccountsAsync(user.Id);
                   if (!mine.Any(s => s.Type.Equals(AccountType.Check)))
                   {
                       var server = await _uw.ServerRepository.GetTestServer();
                       if (server.IsActive)
                       {
 var count = await _uw.AccountRepository.GetLastItemIdAsync();
        var code = Account.GenerateNewAccountCode(count);

        var email = Account.GenerateNewCheckClientEmail();
        var password = Account.GenerateNewAccountPassword();

        var client = new CreateNewClientDto()
        {
            Email = email,
            Username = code.ToLower(),
            Password = password,
            Traffic = 200,
            type_traffic = "mb",
            Multiuser = 1,
            connection_start = 1,
            Desc = $"{user.Id}"
        };

        server.Capacity -= 1;

        var newAccount = new Account()
        {
            AccountCode = code,
            IsActive = true,
            State = AccountState.Active,
            Traffic = 0.2,
            StartsOn = DateTime.Now,
            EndsOn = DateTime.Now.AddDays(1),
            Email = client.Email,
            CreatedOn = DateTime.Now,
            ServerCode = server.Code,
            IsRemoved = false,
            UsedTraffic = 0,
            ExtendNotifyCount = 0,
            LimitIp = client.Multiuser,
            UserName = client.Username.ToLower(),
            Password = password,
            UserId = user.Id
        };

        var result = await _uw.PanelService.CreateNewClientAsync(server, client);

        if (result is not null)
        {
            _uw.ServerRepository.Update(server);
            _uw.AccountRepository.Add(newAccount);
            await _bot.DeleteMessageAsync(chatId, msg.MessageId);
            await _bot.SendCheckClientAsync(_uw,server, newAccount);
        }
                   
                       }
                   }
                }
                else if (step.StartsWith("updatelocation*"))
                {
                    var location = await _uw.LocationRepository.GetLocationByCode(step.Split("*")[1]);
                    if (location is not null)
                    {
                        var property = step.Split("*")[2];
                        switch (property)
                        {
                            case "title":
                                location.Title = message.Text.Fa2En();
                                await _bot.SendTextMessageAsync(user.Id,
                                    "نام کشور موفعیت مکانی با موفقیت تنظیم شد.✅",
                                    replyMarkup: MarkupKeyboards.Main(userInfo.Role));
                                _uw.LocationRepository.Update(location);
                                _uw.SubscriberRepository.ChangeStep(user.Id, $"none");
                                break;
                            case "flat":
                                location.Flat = message.Text.Trim();
                                await _bot.SendTextMessageAsync(user.Id,
                                    "پرچم کشور موفعیت مکانی با موفقیت تنظیم شد.✅",
                                    replyMarkup: MarkupKeyboards.Main(userInfo.Role));
                                _uw.LocationRepository.Update(location);
                                _uw.SubscriberRepository.ChangeStep(user.Id, $"none");
                                break;
                        }

                        await _bot.DeleteMessageAsync(user.Id, int.Parse(step.Split("*")[3]));
                        await _bot.AddNewLocation(user.Id, location);
                    }
                }
                else if (step.Equals("sendiban"))
                {
                    var res = message.Text.Fa2En().ToUpper().Replace("IR", "");
                    if (res.IsNumber())
                    {
                        var match = Regex.Match(res,
                            @"(\b[0-9]{2}(?:[ -]?[0-9]{4}){5}(?!(?:[ -]?[0-9]){3})(?:[ -]?[0-9]{2})?\b)|^$",
                            RegexOptions.IgnoreCase);

                        if (match.Success)
                        {
                            var balance = await _uw.TransactionRepository.GetMineReferralBalance(chatId);
                            _uw.SubscriberRepository.ChangeStep(chatId, "none");
                            await _bot.SendTextMessageAsync(chatId,
                                $".\n" +
                                $"شماره شبا حساب شما با موفقیت ثبت شد. ✅",
                                ParseMode.Html,
                                replyToMessageId: message.MessageId,
                                replyMarkup: MarkupKeyboards.Main(userInfo.Role));

                            var code = Checkout.GenerateNewCheckoutCode();
                            var checkout = new Checkout()
                            {
                                Code = code,
                                UserId = chatId,
                                Amount = balance.Value,
                                State = CheckoutState.Pending,
                                CreatedOn = DateTime.Now,
                                IBan = res.Fa2En()
                            };
                            _uw.CheckoutRepository.Add(checkout);
                            await _bot.SendTextMessageAsync(chatId,
                                $".\n" +
                                $"🔺 تاییده برداشت وجه\n\n" +
                                $"💰 مبلغ <b>{balance.Value.ToIranCurrency().En2Fa()} تومان</b>\n" +
                                $"💳 حساب : <code>IR{res.Fa2En()}</code>\n\n" +
                                $"🔻 آیا اطلاعات فوق را جهت برداشت وجه تایید میکنید؟",
                                ParseMode.Html,
                                replyMarkup: InlineKeyboards.CheckoutConfirmation(code));
                        }
                        else
                        {
                            await _bot.SendTextMessageAsync(chatId,
                                $"لطفا یک شماره شبا معتبر وارد کنید.", replyToMessageId: message.MessageId,
                                replyMarkup: MarkupKeyboards.Cancel());
                        }
                    }
                    else
                    {
                        await _bot.SendTextMessageAsync(chatId,
                            $"لطفا شماره شبا حساب خود را بصورت عددی وارد کنید.", replyToMessageId: message.MessageId,
                            replyMarkup: MarkupKeyboards.Cancel());
                    }
                }
                else if (step.StartsWith("updatecart*"))
                {
                    var cart = _uw.ConfigurationRepository.GetById(int.Parse(step.Split("*")[2]));
                    if (cart is not null)
                        switch (step.Split("*")[1])
                        {
                            case "owner":
                                var owner = message.Text.Trim();
                                if (owner.Length > 3)
                                {
                                    cart.BankAccountOwner = owner;
                                    _uw.ConfigurationRepository.Update(cart);
                                    await _bot.DeleteMessageAsync(chatId, int.Parse(step.Split("*")[3]));
                                    await _bot.SendTextMessageAsync(chatId,
                                        "انم و نام خانوادگی صاحب کارت با موفقیت ویرایش شد.✅️️️️️️️",
                                        replyToMessageId: message.MessageId,
                                        replyMarkup: MarkupKeyboards.Main(userInfo.Role));
                                    await _bot.SendTextMessageAsync(chatId,
                                        $".\n" +
                                        $"💳 ویرایش اطاعات کارت بانکی\n\n" +
                                        $"📍 کارت <b>{cart.Type.ToDisplay()}</b>\n" +
                                        $"👤 <b>{cart.BankAccountOwner}</b>\n" +
                                        $"📌 <b>{cart.CardNumber.En2Fa()}</b>\n\n" +
                                        $"قصد ویرایش کدام یک از اطلاعات را دارید؟", parseMode: ParseMode.Html,
                                        replyMarkup: InlineKeyboards.SingleCartMangement(cart));
                                }
                                else
                                {
                                    await _bot.SendTextMessageAsync(chatId, "نام و نام خانوادگی وارد شده نامعتبر است.",
                                        replyToMessageId: message.MessageId);
                                }

                                _uw.SubscriberRepository.ChangeStep(chatId, $"none");
                                break;
                            case "number":
                                var number = message.Text.Trim().Fa2En();
                                if (number.Length == 16)
                                {
                                    cart.CardNumber = number;
                                    _uw.ConfigurationRepository.Update(cart);
                                    await _bot.DeleteMessageAsync(chatId, int.Parse(step.Split("*")[3]));
                                    await _bot.SendTextMessageAsync(chatId, "شماره کارت با موفقیت ویرایش شد.✅️️️️️️️",
                                        replyToMessageId: message.MessageId,
                                        replyMarkup: MarkupKeyboards.Main(userInfo.Role));
                                    await _bot.SendTextMessageAsync(chatId,
                                        $".\n" +
                                        $"💳 ویرایش اطاعات کارت بانکی\n\n" +
                                        $"📍 کارت <b>{cart.Type.ToDisplay()}</b>\n" +
                                        $"👤 <b>{cart.BankAccountOwner}</b>\n" +
                                        $"📌 <b>{cart.CardNumber.En2Fa()}</b>\n\n" +
                                        $"قصد ویرایش کدام یک از اطلاعات را دارید؟", parseMode: ParseMode.Html,
                                        replyMarkup: InlineKeyboards.SingleCartMangement(cart));
                                }
                                else
                                {
                                    await _bot.SendTextMessageAsync(chatId, "شماره کارت وارد شده نامعتبر است.",
                                        replyToMessageId: message.MessageId);
                                }

                                break;
                        }
                    else await _bot.SendTextMessageAsync(chatId, "کارت مورد نظر یافت نشد.");
                }
                else if (step.Equals("sendremark"))
                {
                    if (message.Text.Length > 4 && !message.Text.Contains(" "))
                    {
                        var remark = message.Text.Fa2En();
                        var remarkIsExist = await _uw.ColleagueRepository.RemarkIsExist(remark);
                        if (!remarkIsExist)
                        {
                            var colleage = await _uw.ColleagueRepository.GetByChatId(chatId);
                            colleage.Tag = remark;
                            _uw.ColleagueRepository.Update(colleage);
                            await _bot.SendTextMessageAsync(chatId, $"ریمارک وارد شده با موفقیت ثبت شد.✅",
                                replyMarkup: MarkupKeyboards.Main(userInfo.Role));
                            _uw.SubscriberRepository.ChangeStep(chatId, "none");
                        }
                        else
                        {
                            await _bot.SendTextMessageAsync(chatId,
                                $"ریمارک وارد شده در سیستم موجود می باشد.✖️\n" +
                                $"لطفا ریمارک دیگری را وارد نمایید :");
                        }
                    }
                    else
                    {
                        await _bot.SendTextMessageAsync(chatId,
                            $"ریمارک نمیتواند کمتر از ۵ حرف یا دارای فاصله باشد.✖️\n" +
                            $"لطفا ریمارک دیگری را وارد نمایید :");
                    }
                }
                else
                {
                    var sub = step.Split("-")[0];
                     switch (sub)
                     {
                         case Constants.ServerConstants:
                             var serverhandler = new ServerMessageHandler(_bot,update,_uw,userInfo);
                             serverhandler.MessageHandlerAsync();
                             break;
                         case Constants.AccountConstants:
                             var accounthandler = new AccountMessageHandler(_bot,update,_uw,userInfo);
                             accounthandler.MessageHandlerAsync();
                             break;
                         case Constants.SubscriberConstatns:
                             var subscriberhandler = new SubscriberMessageHandler(_bot,update,_uw,userInfo);
                             subscriberhandler.MessageHandlerAsync();
                             break;
                         case Constants.AccountReportConstants:
                             var reporthandler = new AccountReportMessageHandler(_bot,update,_uw,userInfo);
                             reporthandler.MessageHandlerAsync();
                             break;
                         case Constants.ServiceConstants:
                             var servicehandler = new ServiceMessageHandler(_bot,update,_uw,userInfo);
                             servicehandler.MessageHandlerAsync();
                             break;
                         case Constants.DiscountConstants:
                             var discounthandler = new DiscountMessageHandler(_bot,update,_uw,userInfo);
                             discounthandler.MessageHandlerAsync();
                             break;
                         case Constants.OrderConstants:
                             var orderhandler = new OrderMessageHandler(_bot,update,_uw,userInfo);
                             orderhandler.MessageHandlerAsync();
                             break;
                         case Constants.CategoryConstants:
                             var categoryhandler = new ServiceCategoryMessageHandler(_bot,update,_uw,userInfo);
                             categoryhandler.MessageHandlerAsync();
                             break;
                     } 
            
                }
            }
            else if (messageType.Equals(MessageType.Photo))
            {
                 var sub = step.Split("-")[0];
                     switch (sub)
                     {
                         case Constants.ServerConstants:
                             var serverhandler = new ServerMessageHandler(_bot,update,_uw,userInfo);
                             serverhandler.MessageHandlerAsync();
                             break;
                         case Constants.AccountConstants:
                             var accounthandler = new AccountMessageHandler(_bot,update,_uw,userInfo);
                             accounthandler.MessageHandlerAsync();
                             break;
                         case Constants.SubscriberConstatns:
                             var subscriberhandler = new SubscriberMessageHandler(_bot,update,_uw,userInfo);
                             subscriberhandler.MessageHandlerAsync();
                             break;
                         case Constants.AccountReportConstants:
                             var reporthandler = new AccountReportMessageHandler(_bot,update,_uw,userInfo);
                             reporthandler.MessageHandlerAsync();
                             break;
                         case Constants.ServiceConstants:
                             var servicehandler = new ServiceMessageHandler(_bot,update,_uw,userInfo);
                             servicehandler.MessageHandlerAsync();
                             break;
                         case Constants.DiscountConstants:
                             var discounthandler = new DiscountMessageHandler(_bot,update,_uw,userInfo);
                             discounthandler.MessageHandlerAsync();
                             break;
                         case Constants.OrderConstants:
                             var orderhandler = new OrderMessageHandler(_bot,update,_uw,userInfo);
                             orderhandler.MessageHandlerAsync();
                             break;
                         case Constants.CategoryConstants:
                             var categoryhandler = new ServiceCategoryMessageHandler(_bot,update,_uw,userInfo);
                             categoryhandler.MessageHandlerAsync();
                             break;
                     } 
            }
            else if (messageType == MessageType.Contact)
            {
                var phonenumber = message.Contact.PhoneNumber.Replace("+98", "0");
                if (step.StartsWith("sub-tocolleague*phonenumber"))
                {
                    phonenumber = phonenumber.StartsWith("98") ? "0" + phonenumber.Substring(2, phonenumber.Length - 2) : phonenumber;
                    var colleague = await _uw.ColleagueRepository.GetByChatId(chatId);
                    if (colleague is not null)
                    {
                        colleague.PhoneNumber = phonenumber.Trim().Fa2En();
                        colleague.Stage = ColleagueStage.HowToSell;
                        _uw.ColleagueRepository.Update(colleague);
                        await _bot.SendTextMessageAsync(chatId, "شماره تماس شما با موفقیت ثبت شد.✅", replyToMessageId: message.MessageId,
                            replyMarkup:MarkupKeyboards.Main(userInfo.Role));
                        await _bot.SendTextMessageAsync(chatId,
                            $"🔑 در حال تکمیل مراحل درخواست همکاری :\n" +
                            $"🔖 #U<code>{colleague.UserId}</code>\n" +
                            $"📌 مرحله سوم از پنجم\n\n" +
                            $"✔️ فروش شما از چه طریق می باشد؟",
                            ParseMode.Html,
                            replyMarkup: SubscriberKeyboards.ColleagueHowToSell());
                        _uw.SubscriberRepository.ChangeStep(chatId, $"none");
                    }
                }
            }
        }
    }
}