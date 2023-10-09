using ConnectBashBot.Commons;
using ConnectBashBot.Telegram.Handlers;
using SSHVpnBot.Components.Accounts;
using SSHVpnBot.Components.Base;
using SSHVpnBot.Components.Colleagues;
using SSHVpnBot.Components.Configurations;
using SSHVpnBot.Components.Discounts;
using SSHVpnBot.Components.Orders.Keyboards;
using SSHVpnBot.Components.Payments;
using SSHVpnBot.Components.Services;
using SSHVpnBot.Components.Subscribers;
using SSHVpnBot.Components.Transactions;
using SSHVpnBot.Repositories.Uw;
using SSHVpnBot.Telegram;
using SSHVpnBot.Telegram.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using File = System.IO.File;

namespace SSHVpnBot.Components.Orders.Handlers;

public class OrderCallbackHandler : QueryHandler
{
    public OrderCallbackHandler(ITelegramBotClient _bot, Update update,
        IUnitOfWork _uw,
        Subscriber subscriber, CancellationToken cancellationToken = default):base(_bot,update,_uw,subscriber)
    {
        
    }
    public override async Task QueryHandlerAsync()
    {
        if (data.StartsWith("factor*"))
        {
            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "سرویس مورد نظر انتخاب شد.");

            if (subscriber.Role == Role.Colleague)
            {
                var service = await _uw.ServiceRepository.GetServiceInfo(data.Replace("factor*", ""));
                if (service is not null && !service.IsRemoved && service.IsActive)
                {
                    await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);

                    await _bot.SendTextMessageAsync(user.Id,
                        $"<b>♻️ بررسی تعداد سفارش</b>\n\n" +
                        $"🔗 اشتراک : <b>{service.GetFullTitle()}</b>\n" +
                        $"{(service.Description.HasValue() ? $"💬 درباره سرویس :\n{service.Description}\n\n" : "\n")}" +
                        $"🔻 تعداد سفارش خود را وارد نمایید :",
                        ParseMode.Html,
                        replyMarkup: MarkupKeyboards.Cancel());

                    _uw.SubscriberRepository.ChangeStep(user.Id, $"{Constants.OrderConstants}-sendcount*{service.Code}");
                }
                else
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "سرویس مورد نظر یافت نشد", true);
                }
            }
            else
            {
                var service = await _uw.ServiceRepository.GetServiceInfo(data.Replace("factor*", ""));
                if (service is not null && !service.IsRemoved && service.IsActive)
                {
                    var payments = _uw.PaymentMethodRepository.GetAll().Where(s => s.IsActive).ToList();

                    var order = new Order()
                    {
                        Amount = service.Price,
                        TotalAmount = service.Price + new Random().Next(100, 999),
                        Count = 1,
                        Type = OrderType.New,
                        State = OrderState.WaitingForPayment,
                        ServiceCode = service.Code,
                        UserId = user.Id,
                        TrackingCode = Order.GenerateNewTrackingCode(),
                        CreatedOn = DateTime.Now
                    };

                    await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
                    await _bot.ReviewOrder(user.Id, payments, service, order);

                    _uw.OrderRepository.Add(order);
                }
                else
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "سرویس مورد نظر یافت نشد", true);
                }
            }
        }
        // else if (data.StartsWith("category*"))
        // {
        //     var category = await _uw.ServiceCategoryRepository.GetByServiceCategoryCode(callBackQuery.Data.Split('*')[1]);
        //     if (category is not null && category.IsActive)
        //     {
        //         if (category.Code.Equals("CAT346100"))
        //         {
        //             var services = await _uw.ServiceRepository.GetServicesByCategoryCodeAsync(category.Code);
        //             if (services.Count is not 0)
        //             {
        //                 await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
        //
        //                 var ownserver = false;
        //                 if (subscriber.Role.Equals(Role.Colleague))
        //                     if (await _uw.ServerRepository.AnyServerForCollague(subscriber.UserId))
        //                         ownserver = true;
        //                 await _bot.SendTextMessageAsync(user.Id,
        //                     ".\n" +
        //                     $"{(category.Description.HasValue() ? $"💬 درباره {category.Title} :\n\n {category.Description}\n\n" : "")}" +
        //                     "🔗 اشتراک مورد نظر خود را انتخاب کنید :", ParseMode.Html,
        //                     replyMarkup: await OrderKeyboards.Services(_uw, subscriber, services, ownserver));
        //             }
        //             else
        //             {
        //                 await _bot.AnswerCallbackQueryAsync(callBackQuery.Id
        //                     , "این سرویس در حال حاضر سرویس فعالی ندارد.", true);
        //             }
        //         }
        //         else
        //         {
        //             await _bot.AnswerCallbackQueryAsync(callBackQuery.Id
        //                 , "این سرویس بزودی اضافه خواهد شد.", true);
        //         }
        //     }
        //     else
        //     {
        //         await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "دسته بندی یافت نشد.", true);
        //     }
        // }
        else if (data.StartsWith("review*"))
        {
            var order = await _uw.OrderRepository.GetByTrackingCode(callBackQuery.Data.Replace("review**", ""));
            if (order is not null)
            {
                var service = await _uw.ServiceRepository.GetServiceByCode(order.ServiceCode);
                var accounts = await _uw.AccountRepository.GetByOrderCodeAsync(order.TrackingCode);
                if (accounts.Count > 0)
                {
                    await _bot.Choosed(callBackQuery);


                    var final_message = $".\n\n" +
                                        $"📌 اطلاعات سفارش #{order.TrackingCode}\n\n" +
                                        $"🔗 <b>{service.GetFullTitle()}</b>\n" +
                                        $"📍 تعداد سفارش : <b>{order.Count.En2Fa()} کانفیگ</b>\n" +
                                        $"⌛️ تاریخ ایجاد : <b>{order.CreatedOn.ConvertToPersianCalendar().En2Fa()}</b>\n" +
                                        $"💳 مبلغ سفارش : <b>{order.TotalAmount.ToIranCurrency().En2Fa()} تومان</b>\n\n\n" +
                                        $"🌐 لیست کانفیگ های سفارش :\n\n";

                    foreach (var acc in accounts.GroupBy(s => s.ServerCode).ToList())
                    {
                        var server = await _uw.ServerRepository.GetServerByCode(acc.Key);
                        if (server is not null)
                        {
                            if (server.IsActive)
                            {
                            }
                            else
                            {
                                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                                    "سرور سفارش مورد نظر غیرفعال می باشد.", true);
                            }
                        }
                    }

                    await _bot.SendTextMessageAsync(user.Id, final_message, ParseMode.Html);
                }
                else
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "اشتراک های سفارش مورد نظر یافت نشد.",
                        true);
                }
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "سفارش مورد نظر یافت نشد.", true);
            }
        }
        // else if (data.Equals("servicecategories"))
        // {
        //     await _bot.Choosed(callBackQuery);
        //     var categories = (await _uw.ServiceCategoryRepository.GetAllCategoriesAsync())
        //      .Where(s => s.IsActive && !s.IsRemoved).ToList();;
        //     if (categories.Count is not 0)
        //     {
        //         await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
        //         var services = _uw.ServiceRepository.GetAll()
        //             .Where(s => s.IsActive && !s.IsRemoved).ToList();
        //         using (var fs = new MemoryStream(File.ReadAllBytes("./medias/cb.jpg")))
        //         {
        //             await _bot.SendPhotoAsync(user.Id, new InputOnlineFile(fs, "radvpn"),
        //                 $"🛒 نوع سرویس مورد نظر خود را جهت سفارش انتخاب نمایید :",
        //                 ParseMode.Html,
        //                 replyMarkup: await OrderKeyboards.Categories(_uw, subscriber, categories,
        //                     services));
        //         }
        //     }
        // }
        else if (data.StartsWith("discount*"))
        {
            var order = await _uw.OrderRepository.GetByTrackingCode(callBackQuery.Data.Split("*")[1]);
            if (order is not null)
            {
                await _bot.SendTextMessageAsync(user.Id, "🔖 کد تخفیف خود را جهت اعمال روی فاکتور وارد کنید :",
                    replyMarkup: MarkupKeyboards.Cancel());
                _uw.SubscriberRepository.ChangeStep(user.Id,
                    $"{Constants.OrderConstants}-discount*{order.TrackingCode}*{callBackQuery.Message.MessageId}");
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "سفارش مورد نظر یافت نشد.", true);
            }
        }
        else if (data.StartsWith("update*"))
        {
            var order = await _uw.OrderRepository.GetByTrackingCode(data.Split("*")[1]);
            if (order is not null)
            {
                var action = data.Split("*")[2];
                var accounts = await _uw.AccountRepository.GetByOrderCodeAsync(order.TrackingCode);
                switch (action)
                {
                    case "resend":
                        await _bot.Choosed(callBackQuery);
                        if (accounts.Count > 0)
                        {
                            foreach (var account in accounts)
                            {
                                //await _bot.SendSellerConfig(_uw, order.UserId, account);
                                Thread.Sleep(800);
                            }

                            await _bot.EditMessageTextAsync(groupId, callBackQuery.Message.MessageId,
                                $"<code>{callBackQuery.Message.Text}</code>\n\n" +
                                $"با موفقیت برای کاربر ارسال شد.✅", ParseMode.Html);
                        }
                        else
                        {
                            //await _bot.SendConfig(_uw, order.UserId, accounts.FirstOrDefault(), false);

                            await _bot.EditMessageTextAsync(groupId, callBackQuery.Message.MessageId,
                                $"<code>{callBackQuery.Message.Text}</code>\n\n" +
                                $"با موفقیت برای کاربر ارسال شد.✅", ParseMode.Html);
                        }

                        break;
                }
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "سفارش مورد نظر یافت نشد.", true);
            }
        }
        else if (data.StartsWith("payment*"))
        {
            var payment_type = await _uw.PaymentMethodRepository.GetPaymentType(int.Parse(data.Split("*")[1]));
            var order = await _uw.OrderRepository.GetByTrackingCode(data.Split("*")[3]);

            var service = await _uw.ServiceRepository.GetServiceByCode(order.ServiceCode);
            Discount discount = null;
            if (order.DiscountNumber.HasValue())
                discount = await _uw.DiscountRepository.GetByDiscountNumberAsync(order.DiscountNumber);

            if (payment_type.Id == 1)
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "روش پرداخت انتخاب شد.");
                Configuration? configs = null;
                if (subscriber.Role.Equals(Role.Colleague))
                    configs = await _uw.ConfigurationRepository.GetByTypeAsync(
                        ConfigurationType.CollleagueSide);
                else
                    configs = await _uw.ConfigurationRepository.GetByTypeAsync(ConfigurationType.CustomerSide);

                await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
                order.PaymentType = PaymentType.Cart;
                order.State = OrderState.WaitingForConfirmRecept;
                await _bot.SendTextMessageAsync(user.Id,
                    $".\n\n" +
                    $"برای سفارش لطفا مبلغ <b>{order.TotalAmount.ToIranCurrency().En2Fa()}</b> تومان\n" +
                    $"به شماره کارت زیر به نام <b>«{configs.BankAccountOwner}»</b> واریز فرمایید:\n\n" +
                    $"💳 <code>{configs.CardNumber}</code>\n\n" +
                    $"سپس روی دکمه‌ی «ارسال تصویر فیش» کلیک کرده و رسید پرداختی خود را ارسال کنید." +
                    $"اکانت شما بلافاصله پس از تایید فیش واریز ارسال خواهد شد.✔️",
                    ParseMode.Html,
                    replyMarkup: OrderKeyboards.SendPaymentRecept(order.TrackingCode));
                // await _bot.SendTextMessageAsync(MainHandler._payments,
                //     $".\n" +
                //     $"📌 پرداخت <b>{order.TotalAmount.ToIranCurrency().En2Fa()} تومان</b> به زودی…\n" +
                //     $"🌀 <b>{(order.Type == OrderType.New ? "خرید" : "تمدید")}</b>\n" +
                //     $"🔗 <b>{order.Count.En2Fa()} کانفیگ {service.GetFullTitle()}</b>\n" +
                //     $"📍 <b>{payment_type.Title}</b>\n" +
                //     $"{(discount is not null ? $"🔖 کد تخفیف {(discount.Type == DiscountType.Amount ? $"{discount.Amount.ToIranCurrency().En2Fa()} تومانی" : $"{discount.Amount.En2Fa()} درصدی")}\n" : "")}" +
                //     $"👤 <a href='tg://user?id={order.UserId}'>#U{order.UserId}</a>\n" +
                //     $".", ParseMode.Html);
            }
            else if (payment_type.Id == 3)
            {
                order.PaymentType = PaymentType.Wallet;

                var balance = await _uw.TransactionRepository.GetMineBalanceAsync(user.Id);
                if (balance >= order.TotalAmount)
                {
                    if (order.State != OrderState.Done)
                    {
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "روش پرداخت انتخاب شد.");
                        if (order.Type == OrderType.Extend)
                        {
                            var account = await _uw.AccountRepository.GetByAccountCode(order.AccountCode);
                            await _bot.ApproveExtendOrder(_uw, order, user, user.Id, callBackQuery);
                            var code = Transaction.GenerateNewDiscountNumber();
                            var transaction = new Transaction()
                            {
                                Amount = order.TotalAmount - order.TotalAmount * 2,
                                CreatedOn = DateTime.Now,
                                UserId = order.UserId,
                                TransactionCode = code,
                                IsRemoved = false,
                                Type = TransactionType.Order
                            };
                            _uw.TransactionRepository.Add(transaction);
                            if (!order.DiscountNumber.HasValue())
                            {
                                var cashbackcode = Transaction.GenerateNewDiscountNumber();
                                var trs = new Transaction()
                                {
                                    Amount = order.TotalAmount / 100 * 5,
                                    CreatedOn = DateTime.Now,
                                    UserId = order.UserId,
                                    TransactionCode = cashbackcode,
                                    IsRemoved = false,
                                    Type = TransactionType.CashBack
                                };
                                _uw.TransactionRepository.Add(trs);
                            }

                            balance = await _uw.TransactionRepository.GetMineBalanceAsync(order.UserId);
                            await _bot.SendTextMessageAsync(MainHandler._payments,
                                $".\n" +
                                $"<b>اطلاعات تمدید ثبت شده 🔵️</b>\n\n" +
                                $"🔖 <b>#{order.TrackingCode}</b>\n" +
                                $"🔗 <b>{order.Count.En2Fa()} کانفیگ {service.GetFullTitle()}</b>\n" +
                                $"🔖 <code>{account.ClientId}</code>\n" +
                                $"💳 <b>{order.TotalAmount.ToIranCurrency().En2Fa()} تومان</b>\n" +
                                $"📅 <b>{order.CreatedOn.ConvertToPersianCalendar().En2Fa()} ساعت {order.CreatedOn.ToString("HH:mm").En2Fa()}</b>\n" +
                                $"👤 <a href='tg://user?id={user.Id}'>#U{user.Id}</a> | {user.FirstName + " " + user.LastName}\n\n" +
                                $"💰 موجودی پس از سفارش : <b>{balance.Value.ToIranCurrency().En2Fa()} تومان</b>\n\n" +
                                $"تایید شده توسط سیستم ✅️",
                                ParseMode.Html);
                        }
                        else
                        {
                            await _bot.ApproveNewOrder(_uw, order, user, user.Id, callBackQuery);
                            if (!order.DiscountNumber.HasValue())
                            {
                                var cashbackcode = Transaction.GenerateNewDiscountNumber();
                                var trans = new Transaction()
                                {
                                    Amount = order.TotalAmount / 100 * 5,
                                    CreatedOn = DateTime.Now,
                                    UserId = order.UserId,
                                    TransactionCode = cashbackcode,
                                    IsRemoved = false,
                                    Type = TransactionType.CashBack
                                };
                                _uw.TransactionRepository.Add(trans);
                            }

                            balance = await _uw.TransactionRepository.GetMineBalanceAsync(order.UserId);
                            await _bot.SendTextMessageAsync(MainHandler._payments,
                                $".\n" +
                                $"<b>اطلاعات سفارش ثبت شده 🟢️</b>\n\n" +
                                $"🔖 <b>#{order.TrackingCode}</b>\n" +
                                $"🔗 <b>{order.Count.En2Fa()} کانفیگ {service.GetFullTitle()}</b>\n" +
                                $"💳 <b>{order.TotalAmount.ToIranCurrency().En2Fa()} تومان</b>\n" +
                                $"📅 <b>{order.CreatedOn.ConvertToPersianCalendar().En2Fa()} ساعت {order.CreatedOn.ToString("HH:mm").En2Fa()}</b>\n" +
                                $"👤 <a href='tg://user?id={user.Id}'>#U{user.Id}</a> | {user.FirstName + " " + user.LastName}\n\n" +
                                $"💰 موجودی پس از سفارش : <b>{balance.Value.ToIranCurrency().En2Fa()} تومان</b>\n\n" +
                                $"تایید شده توسط سیستم ✅️",
                                ParseMode.Html);
                        }

                        await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
                    }
                    else
                    {
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                            $"سفارش شما در وضعیت {order.State.ToDisplay()} قرار دارد.",
                            true);
                    }
                }
                else
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                        $"💰 موجودی کیف پول: {balance.Value.ToIranCurrency().En2Fa()} تومان\n\n" +
                        "⚠️ موجودی کیف پول شما برای پرداخت این فاکتور کافی نمی باشد. لطفاً از طریق حساب کاربری نسبت به شارژ کیف پول خود اقدام و یا از روش پرداخت دیگری استفاده نمائید."
                        , true);
                }
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "روش پرداخت انتخاب شد.");
                await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);

                order.PaymentType = PaymentType.Gateway;
                order.State = OrderState.WaitingForConfirmRecept;

                var rate = 1;
                var amount = (order.TotalAmount / rate).ChangeDecimal(2);
                var payment_link =
                    $"https://digiswap.org/quick?amount={amount}&currency=TRX&address=TXwUbK8kcpcWtKbcwFCgdNQeXoei5Rkpd1";

                await _bot.SendTextMessageAsync(user.Id,
                    $".\n\n" +
                    $"برای سفارش لطفا مبلغ <b>{order.TotalAmount.ToIranCurrency().En2Fa()} تومان</b>\n" +
                    $"را از طریق لینک درگاه پرداخت واسط ارزی جهت خرید سرویس پرداخت نموده :\n\n" +
                    $"🌐 {payment_link}\n\n" +
                    $"📌 در صورت عدم باز شدن درگاه بدون VPN امتحان کنید.\n" +
                    $"سپس روی دکمه‌ی «ارسال تصویر فیش» کلیک کرده و رسید پرداختی خود را ارسال کنید." +
                    $"اکانت شما بلافاصله پس از تایید <b>'اسکرین شات  واریز '</b> ارسال خواهد شد.✔️",
                    ParseMode.Html, disableWebPagePreview: true,
                    replyMarkup: InlineKeyboards.SendGatewayPaymentRecept(payment_link, order.TrackingCode));
            }

            _uw.OrderRepository.Update(order);
        }
        else if (data.StartsWith("sendrecept*"))
        {
            var code = data.Replace("sendrecept*", "");
            if (code.StartsWith("O"))
            {
                var order = await _uw.OrderRepository.GetByTrackingCode(code);
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "ارسال فیش واریزی انتخاب شد.");

                await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
                var service = await _uw.ServiceRepository.GetServiceByCode(order.ServiceCode);
                await _bot.SendTextMessageAsync(user.Id,
                    $".\n\n" +
                    $"🔖 شناسه سفارش : <b>#{order.TrackingCode}</b>\n" +
                    $"🔗 اشتراک : <b>{service.GetFullTitle()}</b>\n" +
                    $"📍 تعداد سفارش : <b>{order.Count.En2Fa()} کانفیگ</b>\n" +
                    $"💰 مبلع پرداختی : <b>{order.TotalAmount.ToIranCurrency().En2Fa()} تومان</b>\n\n" +
                    $"🖼 لطفا تصویر فیش را به صورت تصویر (نه فایل) برای ما ارسال کنید :",
                    ParseMode.Html,
                    replyMarkup: MarkupKeyboards.Cancel());

                _uw.SubscriberRepository.ChangeStep(user.Id, $"{Constants.OrderConstants}-recept*{order.TrackingCode}");
            }
            else if (code.StartsWith("P"))
            {
                var payment = await _uw.PaymentRepository.GetPaymentByCodeAsync(code);
                if (payment is not null)
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "ارسال قیش واریزی انتخاب شد.");

                    await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
                    await _bot.SendTextMessageAsync(user.Id,
                        $".\n\n" +
                        $"🔖 شناسه شارژ حساب : <b>#{payment.PaymentCode}</b>\n" +
                        $"💰 مبلع شارژ : <b>{payment.Amount.ToIranCurrency().En2Fa()} تومان</b>\n\n" +
                        $"🖼 لطفا تصویر فیش را به صورت تصویر (نه فایل) برای ما ارسال کنید :",
                        ParseMode.Html,
                        replyMarkup: MarkupKeyboards.Cancel());

                    _uw.SubscriberRepository.ChangeStep(user.Id, $"{Constants.OrderConstants}-recept*{payment.PaymentCode}");
                }
                else
                {
                    await _bot.SendTextMessageAsync(user.Id, $"اطلاعات پرداخت شما یافت نشد.", ParseMode.Html);
                }
            }
        }
        else if (data.StartsWith("trackorder*"))
        {
            var order = await _uw.OrderRepository.GetByTrackingCode(data.Replace("trackorder*", ""));
            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, $"وضعیت سفارش : {order.State.ToDisplay()}",
                true);
        }
        else if (data.StartsWith("trackpayment*"))
        {
            var payment = await _uw.PaymentRepository.GetPaymentByCodeAsync(data.Replace("trackpayment*", ""));
            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                $"وضعیت پرداخت : {payment.State.ToDisplay()}",
                true);
        }
        else if (data.StartsWith("checkrecept*"))
        {
            var code = data.Split("*")[1];
            if (code.StartsWith("O"))
            {
                var order = await _uw.OrderRepository.GetByTrackingCode(code);
                var answer = data.Split("*")[2];
                if (order.State == OrderState.WaitingForConfirmRecept)
                {
                    if (answer == "approve" && !MainHandler.is_develop)
                    {
                        if (order.Type == OrderType.Extend)
                            await _bot.ApproveExtendOrder(_uw, order, user, user.Id, callBackQuery);
                        else
                            await _bot.ApproveNewOrder(_uw, order, user, user.Id, callBackQuery);
                    }
                    else
                    {
                        await _bot.DeclineOrder(_uw, order, user, user.Id, callBackQuery);
                    }
                }
                else
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                        $"فیش در وضعیت انتظار تایید قرار ندارد.",
                        true);
                }
            }
            else if (code.StartsWith("P"))
            {
                var payment = await _uw.PaymentRepository.GetPaymentByCodeAsync(code);
                var answer = data.Split("*")[2];
                if (payment?.State == PaymentState.Pending)
                {
                    if (answer == "approve" && !MainHandler.is_develop)
                    {
                        var u = await _uw.SubscriberRepository.GetByChatId(payment.UserId);
                        if (u is not null)
                            Task.Run(async () =>
                            {
                                var balance =
                                    await _uw.TransactionRepository.GetMineBalanceAsync(u.UserId);

                                payment.State = PaymentState.Approved;
                                _uw.PaymentRepository.Update(payment);
                                var code = Transaction.GenerateNewDiscountNumber();
                                var transaction = new Transaction()
                                {
                                    Amount = payment.Amount,
                                    CreatedOn = DateTime.Now,
                                    TransactionCode = code,
                                    UserId = u.UserId,
                                    Type = TransactionType.Charge
                                };
                                _uw.TransactionRepository.Add(transaction);

                                balance = await _uw.TransactionRepository.GetMineBalanceAsync(u.UserId);

                                await _bot.UpdateApprovedPaymentReceptMessages(payment, user, user.Id,
                                    balance.Value,
                                    callBackQuery);
                            });
                        else
                            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                                "کاربر مورد نظر یافت نشد.",
                                true);
                    }
                    else
                    {
                        payment.State = PaymentState.Declined;
                        _uw.PaymentRepository.Update(payment);
                        await _bot.DeclinePayment(_uw, payment, user, user.Id, callBackQuery);
                    }
                }
                else
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                        $"فیش در وضعیت انتظار تایید قرار ندارد.",
                        true);
                }
            }
        }
    }
}