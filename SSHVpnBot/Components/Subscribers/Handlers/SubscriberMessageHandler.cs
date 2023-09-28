using ConnectBashBot.Commons;
using SSHVpnBot.Components.Base;
using SSHVpnBot.Components.Colleagues;
using SSHVpnBot.Components.Configurations;
using SSHVpnBot.Components.Orders.Keyboards;
using SSHVpnBot.Components.Payments;
using SSHVpnBot.Components.Subscribers.Keyboards;
using SSHVpnBot.Repositories.Uw;
using SSHVpnBot.Telegram.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Components.Subscribers.Handlers;
public class SubscriberMessageHandler : MessageHandler
{
    public SubscriberMessageHandler(ITelegramBotClient _bot, Update update, IUnitOfWork _uw,
        Subscriber subscriber, CancellationToken cancellationToken = default)
        :base(_bot,update,_uw,subscriber)
    {
        
    }
    public override async Task MessageHandlerAsync()
    {
        var chatId = user.Id;
        if (step.Equals("chargewallet"))
        {
            if (message.Text.Fa2En().IsNumber())
            {
                if (int.Parse(message.Text.Fa2En()) < 50000)
                {
                    await _bot.SendTextMessageAsync(chatId,
                        "حداقل مقدار شارژ کیف پول <b>۵۰،۰۰۰ تومان</b> می باشد.",
                        ParseMode.Html,
                        replyToMessageId: message.MessageId);
                }
                else
                {
                    var payment_code = Payment.GenerateNewPaymentCode();

                    var payment = new Payment()
                    {
                        Amount = decimal.Parse(message.Text.Fa2En()),
                        State = PaymentState.Pending,
                        CreatedOn = DateTime.Now,
                        PaymentCode = payment_code,
                        IsRemoved = false,
                        UserId = chatId
                    };
                    _uw.PaymentRepository.Add(payment);

                    Configuration? configs = null;
                    if (subscriber.Role.Equals(Role.Colleague))
                        configs = await _uw.ConfigurationRepository.GetByTypeAsync(ConfigurationType
                            .CollleagueSide);
                    else
                        configs = await _uw.ConfigurationRepository.GetByTypeAsync(ConfigurationType
                            .CustomerSide);

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
                }
            }
            else
            {
                await _bot.SendTextMessageAsync(chatId, "مقدار شارژ تومانی خود را بصورت عددی وارد نمایید.✖️",
                    replyToMessageId: message.MessageId);
            }
        }
        else if (step.StartsWith("tocolleague*"))
        {
            var section = step.Split("*")[1];
            switch (section)
            {
                case "fullname":
                    if (message.Text.Length > 5)
                    {
                        var colleague = await _uw.ColleagueRepository.GetByChatId(chatId);
                        if (colleague is not null)
                        {
                            colleague.FullName = message.Text.Trim();
                            colleague.Stage = ColleagueStage.PhoneNumber;
                            _uw.ColleagueRepository.Update(colleague);
                            await _bot.DeleteMessageAsync(chatId, int.Parse(step.Split("*")[2]));
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
                        }
                    }
                    else await _bot.SendTextMessageAsync(chatId, $"لطفا یک نام و نام خانوادگی صحیح وارد کنید.",replyToMessageId:message.MessageId);
                    break;
                case "phonenumber":
                    if (message.Text.Equals("مرحله بعد 👉️"))
                    {
                        var colleague = await _uw.ColleagueRepository.GetByChatId(chatId);
                        if (colleague is not null)
                        {
                            colleague.Stage = ColleagueStage.HowToSell;
                            _uw.ColleagueRepository.Update(colleague);
                            _uw.SubscriberRepository.ChangeStep(chatId,"none");
                            await _bot.SendTextMessageAsync(chatId, $"شما با موفقیت از مرحله ارسال شماره تماس عبور کردید.✅",
                                replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                            await _bot.SendTextMessageAsync(subscriber.UserId,
                                $"🔑 در حال تکمیل مراحل درخواست همکاری :\n" +
                                $"🔖 #U<code>{colleague.UserId}</code>\n" +
                                $"📌 مرحله سوم از پنجم\n\n" +
                                $"✔️ فروش شما از چه طریق می باشد؟",
                                ParseMode.Html,
                                replyMarkup: SubscriberKeyboards.ColleagueHowToSell());
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        else if (step.StartsWith("sendticket*"))
        {
            var u = await _uw.SubscriberRepository.GetByChatId(long.Parse(step.Replace("sendticket*", "")));
            if (u is not null)
            {
                _uw.SubscriberRepository.ChangeStep(chatId, $"none");
                await _bot.SendTextMessageAsync(u.UserId,
                    $".\n" +
                    $"💌 شما یک پیغام خصوصی از مدیریت دریافت کردید.\n\n" +
                    $"{message.Text}",
                    ParseMode.Html);
                await _bot.SendTextMessageAsync(chatId, $"پیام شما با موفقیت به کاربر ارسال شد.✅",
                    replyToMessageId: message.MessageId,
                    replyMarkup: MarkupKeyboards.Main(subscriber.Role));
            }
        }
        else if (step.StartsWith("sendtoall*"))
        {
            var contatcs = await _uw.SubscriberRepository.GetAllChatIds();

            var msg = await _bot.SendTextMessageAsync(user.Id, "در حل ارسال..⌛️");
            var counter = 1;

            if (message.Text.Length > 15)
                Task.Run(async () =>
                {
                    if (step.Replace("sendtoall*", "") == "all")
                    {
                        for (var i = 0; i < contatcs.Count; i++)
                        {
                            try
                            {
                                await _bot.SendTextMessageAsync(contatcs[i].UserId, message.Text,
                                    ParseMode.Html);
                                counter++;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }

                            if (i % 10 == 0)
                            {
                                await _bot.EditMessageTextAsync(user.Id, msg.MessageId,
                                    $"تا کنون به {counter} کاربر ارسال شده است..♻️");
                                Thread.Sleep(2000);
                            }
                        }

                        await _bot.SendTextMessageAsync(user.Id,
                            $"پیام همگانی با موفقیت به {counter} کاربر ارسال شد.✅", replyMarkup:
                            MarkupKeyboards.Main(subscriber.Role));
                    }
                    else if (step.Replace("sendtoall*", "") == "sellers")
                    {
                        for (var i = 0; i < contatcs.Where(s => s.Role == Role.Colleague).Count(); i++)
                            try
                            {
                                await _bot.SendTextMessageAsync(contatcs[i].UserId, message.Text,
                                    ParseMode.Html);
                                counter++;
                                if (i % 10 == 0)
                                {
                                    await _bot.EditMessageTextAsync(user.Id, msg.MessageId,
                                        $"تا کنون به {counter} فروشنده ارسال شده است..♻️");
                                    Thread.Sleep(2000);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }

                        await _bot.SendTextMessageAsync(user.Id,
                            $"پیام همگانی با موفقیت به {counter} فروشنده ارسال شد.✅", replyMarkup:
                            MarkupKeyboards.Main(subscriber.Role));
                    }
                    else if (step.Replace("sendtoall*", "") == "subscribers")
                    {
                        for (var i = 0; i < contatcs.Where(s => s.Role == Role.Subscriber).Count(); i++)
                        {
                            try
                            {
                                await _bot.SendTextMessageAsync(contatcs[i].UserId, message.Text,
                                    ParseMode.Html);
                                counter++;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                                Console.WriteLine(e.Message);
                            }

                            if (i % 10 == 0)
                            {
                                await _bot.EditMessageTextAsync(user.Id, msg.MessageId,
                                    $"تا کنون به {counter} کاربر ارسال شده است..♻️");
                                Thread.Sleep(2000);
                            }
                        }

                        await _bot.SendTextMessageAsync(user.Id,
                            $"پیام همگانی با موفقیت به {counter} کاربر ارسال شد.✅", replyMarkup:
                            MarkupKeyboards.Main(subscriber.Role));
                    }
                    else if (step.Replace("sendtoall*", "") == "notusetoffer")
                    {
                        var notgotoffercontacts = await _uw.OrderRepository.NotGotDiscountBefore();

                        for (var i = 0; i < notgotoffercontacts.Count; i++)
                        {
                            try
                            {
                                await _bot.SendTextMessageAsync(notgotoffercontacts[i], message.Text,
                                    ParseMode.Html);
                                counter++;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }

                            if (i % 10 == 0)
                            {
                                await _bot.EditMessageTextAsync(user.Id, msg.MessageId,
                                    $"تا کنون به {counter} کاربر ارسال شده است.♻️");
                                Thread.Sleep(2000);
                            }
                        }

                        await _bot.SendTextMessageAsync(user.Id,
                            $"پیام همگانی با موفقیت به {counter} کاربر ارسال شد.✅", replyMarkup:
                            MarkupKeyboards.Main(subscriber.Role));
                    }
                    else if (step.Replace("sendtoall*", "") == "factors")
                    {
                        var factors_notpaid = await _uw.OrderRepository.FactorsNotPaid();

                        for (var i = 0; i < factors_notpaid.Count; i++)
                        {
                            try
                            {
                                var has_order = await _uw.OrderRepository.AnyPaidOrder(factors_notpaid[i]);
                                if (!has_order)
                                {
                                    await _bot.SendTextMessageAsync(factors_notpaid[i], message.Text,
                                        ParseMode.Html);
                                    counter++;
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }

                            if (i % 10 == 0)
                            {
                                await _bot.EditMessageTextAsync(user.Id, msg.MessageId,
                                    $"تا کنون به {counter} کاربر ارسال شده است.♻️");
                                Thread.Sleep(2000);
                            }
                        }

                        await _bot.SendTextMessageAsync(user.Id,
                            $"پیام همگانی با موفقیت به {counter} کاربر ارسال شد.✅", replyMarkup:
                            MarkupKeyboards.Main(subscriber.Role));
                    }
                    else if (step.Replace("sendtoall*", "") == "notgettest")
                    {
                        var gottestcontacts = await _uw.AccountRepository.GotTestUsers();

                        var notGettTestBefore = new List<long>();
                        foreach (var contatc in contatcs)
                            if (gottestcontacts.Any(s => s != contatc.UserId))
                                notGettTestBefore.Add(contatc.UserId);

                        for (var i = 0; i < notGettTestBefore.Count(); i++)
                        {
                            try
                            {
                                await _bot.SendTextMessageAsync(notGettTestBefore[i], message.Text,
                                    ParseMode.Html);
                                counter++;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }

                            if (i % 10 == 0)
                            {
                                await _bot.EditMessageTextAsync(user.Id, msg.MessageId,
                                    $"تا کنون به {counter} کاربر ارسال شده است.♻️");
                                Thread.Sleep(2000);
                            }
                        }

                        await _bot.SendTextMessageAsync(user.Id,
                            $"پیام همگانی با موفقیت به {counter} کاربر ارسال شد.✅", replyMarkup:
                            MarkupKeyboards.Main(subscriber.Role));
                    }
                    else if (step.Replace("sendtoall*", "") == "activeconfigs")
                    {
                        var activeconfigs = await _uw.AccountRepository.GetActiveOnes();

                        for (var i = 0; i < activeconfigs.Count(); i++)
                        {
                            try
                            {
                                await _bot.SendTextMessageAsync(activeconfigs[i], message.Text,
                                    ParseMode.Html);
                                counter++;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }

                            if (i % 10 == 0)
                            {
                                await _bot.EditMessageTextAsync(user.Id, msg.MessageId,
                                    $"تا کنون به {counter} کاربر ارسال شده است.♻️");
                                Thread.Sleep(2000);
                            }
                        }

                        await _bot.SendTextMessageAsync(user.Id,
                            $"پیام همگانی با موفقیت به {counter} کاربر ارسال شد.✅", replyMarkup:
                            MarkupKeyboards.Main(subscriber.Role));
                    }

                    await _bot.DeleteMessageAsync(user.Id, msg.MessageId);
                    _uw.SubscriberRepository.ChangeStep(user.Id, "none");
                });
            else
                await _bot.SendTextMessageAsync(user.Id, $"حداقل طول پیام همگانی می بایست ۱۵ کاراکتر باشد.",
                    replyToMessageId: message.MessageId);
        }
    }
}