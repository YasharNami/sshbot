using ConnectBashBot.Commons;
using ConnectBashBot.Telegram.Handlers;
using SSHVpnBot.Components.Colleagues;
using SSHVpnBot.Components.Discounts;
using SSHVpnBot.Components.Orders.Keyboards;
using SSHVpnBot.Components.Services;
using SSHVpnBot.Components.Subscribers;
using SSHVpnBot.Repositories.Uw;
using SSHVpnBot.Telegram.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using MessageHandler = SSHVpnBot.Components.Base.MessageHandler;

namespace SSHVpnBot.Components.Orders.Handlers;

public class OrderMessageHandler : MessageHandler
{
    public OrderMessageHandler(ITelegramBotClient _bot, Update update, IUnitOfWork _uw,
        Subscriber subscriber, CancellationToken cancellationToken = default)
        :base(_bot,update,_uw,subscriber)
    {
        
    }
    public override async Task MessageHandlerAsync()
    {
        switch (message.Type)
        {
            case MessageType.Text: 
                if (step.StartsWith("sendcount*"))
                {
                    if (message.Text.Fa2En().IsNumber())
                    {
                        if (int.Parse(message.Text.Fa2En()) > 30)
                        {
                            await _bot.SendTextMessageAsync(user.Id, "حداکثر تعداد در هر سفارش ۳۰ عدد می باشد.");
                        }
                        else
                        {
                            var service =
                                await _uw.ServiceRepository.GetServiceByCode(step.Replace("sendcount*", ""));
                           var colleague = await _uw.ColleagueRepository.GetByChatId(user.Id);
                                if (int.Parse(message.Text.Fa2En()) >= (colleague.Level == ColleagueLevel.Base ? 3 : 1))
                                {
                                    var payments = _uw.PaymentMethodRepository.GetAll().Where(s => s.IsActive).ToList();

                                    var count = int.Parse(message.Text.Fa2En().Replace("-", ""));
                                    var order = new Order()
                                    {
                                        Amount = service.SellerPrice,
                                        TotalAmount = (service.SellerPrice * count) + new Random().Next(100, 999),
                                        State = OrderState.WaitingForPayment,
                                        Type = OrderType.New,
                                        ServiceCode = service.Code,
                                        Count = count,
                                        UserId = user.Id,
                                        TrackingCode = Order.GenerateNewTrackingCode()
                                    };
                                    await _bot.SendTextMessageAsync(user.Id, $"تعداد سفارش شما با موفقیت ثبت شد.✅",
                                        ParseMode.Html, replyToMessageId:
                                        message.MessageId, replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                                    await _bot.SendTextMessageAsync(user.Id,
                                        $".\n" +
                                        $"<b>♻️ بررسی و تایید سفارش</b>\n\n" +
                                        $"🔗 اشتراک : {service.GetFullTitle()}\n" +
                                        $"🔻 تعداد سفارش : {message.Text.Replace("-", "").En2Fa()} عدد\n" +
                                        $"💰 قیمت فاکتور : {order.TotalAmount.ToIranCurrency().En2Fa()} تومان\n\n" +
                                        $"▫️نحوه‌ی پرداخت خود را انتخاب کنید:".En2Fa(),
                                        ParseMode.Html,
                                        replyMarkup: OrderKeyboards.PaymentMethods(payments, service,
                                            order.TrackingCode));
                                    _uw.OrderRepository.Add(order);
                                }
                                else
                                {
                                    await _bot.SendTextMessageAsync(user.Id,
                                        "حداقل تعداد در هر سفارش برای همکار ۳ عدد می باشد.");
                                }
                        }
                    }
                    else
                    {
                        await _bot.SendTextMessageAsync(user.Id, "تعداد سفارش خود را بصورت عددی وارد نمایید.✖️",
                            replyToMessageId: message.MessageId);
                    }
                }
                else if (step.StartsWith("discount*"))
                {
                    var order = await _uw.OrderRepository.GetByTrackingCode(step.Split("*")[1]);
                    if (order is not null)
                    {
                        if (!order.DiscountNumber.HasValue())
                        {
                            var discount =
                                await _uw.DiscountRepository.GetByCodeAsync(message.Text.Trim().Fa2En().ToLower());
                            if (discount is not null)
                            {
                                if (discount.UserId is not 0)
                                    if (!discount.UserId.Equals(order.UserId))
                                    {
                                        await _bot.SendTextMessageAsync(user.Id, "کد تخفیف وارد شده نامعتبر است.✖️",
                                            replyToMessageId: message.MessageId);
                                        return;
                                    }

                                var useBefore =
                                    await _uw.OrderRepository.UseOffCodeBefore(user.Id, discount.DiscountNumber);
                                if (!useBefore)
                                {
                                    var orders =
                                        await _uw.OrderRepository.GetAllByDiscountNumber(discount.DiscountNumber);

                                    if (discount.ExpiredOn <= DateTime.Now ||
                                        (discount.UsageLimitation != 0 && orders >= discount.UsageLimitation))
                                    {
                                        await _bot.SendTextMessageAsync(user.Id, "کد تخفیف وارد شده نامعتبر است.✖️",
                                            replyToMessageId: message.MessageId);
                                    }
                                    else
                                    {
                                        if (discount.ServiceCode.HasValue() &&
                                            !order.ServiceCode.Equals(discount.ServiceCode))
                                        {
                                            await _bot.SendTextMessageAsync(user.Id,
                                                $"کد تخفیف وارد شده به سرویس سفارش داده شده تعلق ندارد.",
                                                replyToMessageId: message.MessageId);
                                            return;
                                        }

                                        var off = discount.Type == DiscountType.Amount
                                            ? discount.Amount
                                            : order.TotalAmount / 100 * discount.Amount;
                                        if (discount.MaxAmountOfPercent is not 0)
                                            if (off >= discount.MaxAmountOfPercent)
                                                off = discount.MaxAmountOfPercent;
                                        order.TotalAmount = order.TotalAmount - off + new Random().Next(100, 999);
                                        order.DiscountNumber = discount.DiscountNumber;
                                        _uw.OrderRepository.Update(order);

                                        await _bot.DeleteMessageAsync(user.Id, int.Parse(step.Split("*")[2]));

                                        if (discount.Type == DiscountType.Percent)
                                            await _bot.SendTextMessageAsync(user.Id,
                                                $"کد تخفیف <b>{discount.Amount.ToString().En2Fa()}</b> درصدی\n" +
                                                $"با موفقیت روی فاکتور شما اعمال شد. ✅",
                                                ParseMode.Html,
                                                replyToMessageId: message.MessageId,
                                                replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                                        else
                                            await _bot.SendTextMessageAsync(user.Id,
                                                $"کد تخفیف به ارزش <b>{discount.Amount.ToIranCurrency().En2Fa()} تومان</b>\n" +
                                                $"با موفقیت روی فاکتور شما اعمال شد. ✅",
                                                ParseMode.Html,
                                                replyToMessageId: message.MessageId,
                                                replyMarkup: MarkupKeyboards.Main(subscriber.Role));

                                        var payments = _uw.PaymentMethodRepository.GetAll().Where(s => s.IsActive)
                                            .ToList();

                                        var service = await _uw.ServiceRepository.GetServiceByCode(order.ServiceCode);
                                        await _bot.ReviewOrderAfterDiscount(user.Id, payments, service, order);
                                        _uw.SubscriberRepository.ChangeStep(user.Id, "none");
                                    }
                                }
                                else
                                {
                                    await _bot.SendTextMessageAsync(user.Id,
                                        "کد تخفیف وارد شده قبلا مورد استفاده قرار گرفته است.✖️",
                                        replyToMessageId: message.MessageId);
                                }
                            }
                            else
                            {
                                await _bot.SendTextMessageAsync(user.Id, "کد تخفیف وارد شده نامعتبر است.✖️",
                                    replyToMessageId: message.MessageId);
                            }
                        }
                        else
                        {
                            await _bot.SendTextMessageAsync(user.Id, "کدتخفیف برای این فاکتور وارد شده است.✖️",
                                replyToMessageId: message.MessageId);
                        }
                    }
                    else
                    {
                        await _bot.SendTextMessageAsync(user.Id, "سفارش مورد نظر یافت نشد.");
                    }
                }
                break;
            case MessageType.Photo:
                if (step.StartsWith("recept*"))
                {
                    var code = step.Replace("recept*", "");
                    var msg = await _bot.ForwardMessageAsync(MainHandler._payments,
                        user.Id, message.MessageId);
                    Subscriber? referred = null;
                    if (subscriber.Referral.HasValue() && subscriber.Referral.IsNumber())
                    {
                        referred = await _uw.SubscriberRepository.GetByChatId(long.Parse(subscriber.Referral));
                    }
                    if (code.StartsWith("O"))
                    {
                        var order = await _uw.OrderRepository.GetByTrackingCode(step.Replace("recept*", ""));
                        var service = await _uw.ServiceRepository.GetServiceByCode(order.ServiceCode);
                        var orders = await _uw.OrderRepository.GetMineOrders(user.Id);
                        Discount? discount = null;
                        if (order.DiscountNumber.HasValue())
                            discount = await _uw.DiscountRepository.GetByDiscountNumberAsync(order.DiscountNumber);
                        if (order.Type == OrderType.New)
                        {
                           
                            await _bot.SendTextMessageAsync(MainHandler._payments,
                                $".\n" +
                                $"<b>سفارش جدید ثبت شد 🟢️</b>\n\n" +
                                $"🔖 <b>#{order.TrackingCode}</b>\n" +
                                $"🔗 <b>{order.Count.En2Fa()} کانفیگ {service?.GetFullTitle()}</b>\n" +
                                $"💰 <b>{order.TotalAmount.ToIranCurrency().En2Fa()} تومان</b>\n" +
                                $"⌛️ <b>{order.CreatedOn.ConvertToPersianCalendar().En2Fa()} {order.CreatedOn.ToString("HH:mm").En2Fa()}</b>\n" +
                                $"👤 <a href='tg://user?id={user.Id}'>{user.FirstName + " " + user.LastName}</a> | <b>#U{order.UserId}</b>\n" +
                                $"📌 سفارش <b>{(orders.Count + 1).En2Fa()}</b> می باشد.\n\n" +
                                $"{(referred is not null ? $"🗣 معرف : #<a href='tg://user?id={referred.UserId}'>{referred.UserId}</a>\n" : "")}" +
                                $"{(discount is not null ? $"🎁 کد تخفیف <b>{discount.Code} - {(discount.Type == DiscountType.Amount ? $"{discount.Amount.ToIranCurrency().En2Fa()} تومانی" : $"{discount.Amount.En2Fa()} درصدی")}</b>\n\n" : "")}"+
                                $"♻️ آیا اطلاعات فوق را جهت تحویل سرویس تایید میکنید؟ ",
                                ParseMode.Html,
                                replyToMessageId: msg.MessageId,
                                replyMarkup: OrderKeyboards.ReceptConfirmagtion(order.TrackingCode));
                        }
                        else
                        {
                            var account = await _uw.AccountRepository.GetByAccountCode(order.AccountCode);
                            await _bot.SendTextMessageAsync(MainHandler._payments,
                                $"<b>تمدید جدید ثبت شد 🔵️</b>\n\n" +
                                $"🔖 <b>#{order.TrackingCode}</b>\n" +
                                $"🔗 <b>{service?.GetFullTitle()}</b>\n" +
                                $"🔖 <code>{account?.ClientId}</code>\n" +
                                $"💰 <b>{order.TotalAmount.ToIranCurrency().En2Fa()} تومان</b>\n" +
                                $"⌛️ <b>{order.CreatedOn.ConvertToPersianCalendar().En2Fa()}" +
                                $" ساعت {order.CreatedOn.ToString("HH:mm").En2Fa()}</b>\n" +
                                $"👤 <a href='tg://user?id={user.Id}'>{user.FirstName + " " + user.LastName}</a> | <b>#U{order.UserId}</b>\n\n" +
                                $"📌 سفارش <b>{(orders.Count + 1).En2Fa()}</b> می باشد.\n\n" +
                                $"{(referred is not null ? $"🗣 معرف : #<a href='tg://user?id={referred.UserId}'>{referred.UserId}</a>\n" : "")}" +
                                $"{(discount is not null ? $"🎁 کد تخفیف <b>{discount.Code} - {(discount.Type == DiscountType.Amount ? $"{discount.Amount.ToIranCurrency().En2Fa()} تومانی" : $"{discount.Amount.En2Fa()} درصدی")}</b>\n\n" : "")}"+
                                $"♻️ آیا اطلاعات فوق را جهت تمدید سرویس تایید میکنید؟ ",
                                ParseMode.Html,
                                replyToMessageId: msg.MessageId,
                                replyMarkup: OrderKeyboards.ReceptConfirmagtion(order.TrackingCode));
                        }

                        await _bot.SendTextMessageAsync(user.Id, $"تصویر فیش واریزی شما با موفقیت ثبت شد.",
                            replyToMessageId: message.MessageId,
                            replyMarkup: MarkupKeyboards.Main(subscriber.Role));

                        await _bot.SendTextMessageAsync(user.Id,
                            $"سفارش شما با کد پیگیری <code>#{order.TrackingCode}</code> ثبت شد. ✅\n" +
                            $"اوپراتوهای ما حداکثر تا <b>۱۲ ساعت</b> سفارش شما را بررسی میکنند و بعد از تایید کانفیگ برای شما ارسال خواهد شد.\n\n" +
                            $"در صورتی که بعد از این زمان کانفیگ برای شما ارسال نشد می‌توانید با زدن دکمه‌ی زیر سفارش خود را پیگیری کنید",
                            ParseMode.Html,
                            replyMarkup: OrderKeyboards.TrackOrder(order.TrackingCode));
                    }
                    else if (code.StartsWith("P"))
                    {
                        var payment = await _uw.PaymentRepository.GetPaymentByCodeAsync(code);
                        if (payment is not null)
                        {
                            await _bot.SendTextMessageAsync(MainHandler._payments,
                                $".\n" +
                                $"<b>اطلاعات شارژ کیف پول ثبت شده 💰</b>\n\n" +
                                $"🔖 <b>#{payment.PaymentCode}</b>\n" +
                                $"💳 <b>{payment.Amount.ToIranCurrency().En2Fa()} تومان</b>\n" +
                                $"📅 <b>{payment.CreatedOn.ConvertToPersianCalendar().En2Fa()} ساعت {payment.CreatedOn.ToString("HH:mm").En2Fa()}</b>\n" +
                                $"👤 <code>#U{user.Id}</code> | <a href='tg://user?id={user.Id}'>{user.FirstName} {user.LastName}</a>\n" +
                                $"{(referred is not null ? $"🗣 معرف : #<a href='tg://user?id={referred.UserId}'>{referred.UserId}</a>\n" : "")}" +
                                $"♻️ آیا اطلاعات فوق را جهت شارژ حساب تایید میکنید؟",
                                ParseMode.Html,
                                replyToMessageId: msg.MessageId,
                                replyMarkup: OrderKeyboards.ReceptConfirmagtion(payment.PaymentCode));

                            await _bot.SendTextMessageAsync(user.Id, $"تصویر فیش واریزی شما با موفقیت ثبت شد.",
                                replyToMessageId: message.MessageId,
                                replyMarkup: MarkupKeyboards.Main(subscriber.Role));

                            await _bot.SendTextMessageAsync(user.Id,
                                $"درخواست شارژ شما با کد پیگیری <code>#{payment.PaymentCode}</code> ثبت شد. ✅\n" +
                                $"اپراتوهای ما حداکثر تا <b>۱۲ ساعت</b> سفارش شما را بررسی میکنند و بعد از تایید حساب کاربری شما شارژ خواهد شد.\n\n" +
                                $"در صورتی که بعد از این زمان شارژ حساب انجام نشد می‌توانید با زدن دکمه‌ی زیر سفارش خود را پیگیری کنید",
                                ParseMode.Html,
                                replyMarkup: OrderKeyboards.TrackPayment(payment.PaymentCode));
                        }
                        else
                        {
                            await _bot.SendTextMessageAsync(user.Id,
                                $"اطلاعات پرداخت شما یافت نشد.",
                                ParseMode.Html);
                        }
                    }

                    _uw.SubscriberRepository.ChangeStep(user.Id, "none");
                }

                break;
            default:
                break;
        }
    }
}