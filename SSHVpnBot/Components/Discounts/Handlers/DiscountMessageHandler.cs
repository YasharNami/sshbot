using ConnectBashBot.Commons;
using SSHVpnBot.Components.Base;
using SSHVpnBot.Components.Subscribers;
using SSHVpnBot.Repositories.Uw;
using SSHVpnBot.Telegram.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Components.Discounts.Handlers;

public class DiscountMessageHandler : MessageHandler
{
    public DiscountMessageHandler(ITelegramBotClient _bot, Update update, IUnitOfWork _uw,
        Subscriber subscriber, CancellationToken cancellationToken = default)
        :base(_bot,update,_uw,subscriber)
    {
        
    }
    public override async Task MessageHandlerAsync()
    {
        switch (message.Type)
        {
            case MessageType.Text:
                if (step.StartsWith("update*"))
                {
                    var discount = await _uw.DiscountRepository.GetByIdAsync(int.Parse(step.Split("*")[1]));
                    var property = step.Split("*")[2];
                    if (discount is not null)
                        switch (property)
                        {
                            case "code":
                                if (message.Text.Length > 3)
                                {
                                    if (await _uw.DiscountRepository.ExistByCode(message.Text, discount.Id) == false)
                                    {
                                        discount.Code = message.Text.Trim().Fa2En().ToLower();
                                        _uw.DiscountRepository.Update(discount);
                                        await _bot.SendTextMessageAsync(user.Id, "کد با موفقیت ویرایش شد.✅",
                                            replyToMessageId: message.MessageId,
                                            replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                                        await _bot.DeleteMessageAsync(user.Id, int.Parse(step.Split("*")[3]));
                                        await _bot.AddNewDiscount(user.Id, discount);
                                    }
                                    else
                                    {
                                        await _bot.SendTextMessageAsync(user.Id,
                                            "کد تخفیف وارد شده در سیستم موجود می باشد.",
                                            replyToMessageId: message.MessageId, replyMarkup: MarkupKeyboards.Cancel());
                                    }
                                }
                                else
                                {
                                    await _bot.SendTextMessageAsync(user.Id, "لطفا یک کد تخفیف معتبر وارد کنید.",
                                        replyToMessageId: message.MessageId, replyMarkup: MarkupKeyboards.Cancel());
                                }

                                break;
                            case "user":
                                if (message.Text.Length > 5)
                                {
                                    if (message.Text.Fa2En().IsNumber())
                                    {
                                        var u = await _uw.SubscriberRepository.GetByChatId(
                                            long.Parse(message.Text.Fa2En()));
                                        if (u is not null)
                                        {
                                            //discount.UserId = long.Parse(message.Text.Trim().Fa2En());
                                            _uw.DiscountRepository.Update(discount);
                                            await _bot.SendTextMessageAsync(user.Id,
                                                $"کد تخفیف با موفقیت به {u.FullName} اختصاص داده شد.✅",
                                                replyToMessageId: message.MessageId,
                                                replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                                            await _bot.DeleteMessageAsync(user.Id, int.Parse(step.Split("*")[3]));
                                            await _bot.AddNewDiscount(user.Id, discount);
                                        }
                                        else
                                        {
                                            await _bot.SendTextMessageAsync(user.Id,
                                                "کاربر مورد نظر یاقت نشد.",
                                                replyToMessageId: message.MessageId,
                                                replyMarkup: MarkupKeyboards.Cancel());
                                        }
                                    }
                                    else
                                    {
                                        await _bot.SendTextMessageAsync(user.Id, "لطفا یک کد تخفیف معتبر وارد کنید.",
                                            replyToMessageId: message.MessageId, replyMarkup: MarkupKeyboards.Cancel());
                                    }
                                }
                                else
                                {
                                    await _bot.SendTextMessageAsync(user.Id, "لطفا یک مقدار عددی معتبر وارد کنید.",
                                        replyToMessageId: message.MessageId);
                                }

                                break;
                            case "maxpercentamount":
                                if (message.Text.Fa2En().IsNumber())
                                {
                                    // discount.MaxAmountOfPercent = int.Parse(message.Text.Fa2En());
                                    _uw.DiscountRepository.Update(discount);
                                    await _bot.SendTextMessageAsync(user.Id,
                                        "میزان سقف درصد تخفیف با موفقیت ویرایش شد.✅",
                                        replyToMessageId: message.MessageId,
                                        replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                                    await _bot.DeleteMessageAsync(user.Id, int.Parse(step.Split("*")[3]));
                                    await _bot.AddNewDiscount(user.Id, discount);
                                }
                                else
                                {
                                    await _bot.SendTextMessageAsync(user.Id, "لطفا یک مقدار عددی معتبر وارد کنید.",
                                        replyToMessageId: message.MessageId, replyMarkup: MarkupKeyboards.Cancel());
                                }

                                break;
                                break;
                            case "amount":
                                if (message.Text.Fa2En().IsNumber())
                                {
                                    discount.Amount = decimal.Parse(message.Text.Fa2En());
                                    _uw.DiscountRepository.Update(discount);
                                    await _bot.SendTextMessageAsync(user.Id, "مقدار کدتخفیف با موفقیت ویرایش شد.✅",
                                        replyToMessageId: message.MessageId,
                                        replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                                    await _bot.DeleteMessageAsync(user.Id, int.Parse(step.Split("*")[3]));
                                    await _bot.AddNewDiscount(user.Id, discount);
                                }
                                else
                                {
                                    await _bot.SendTextMessageAsync(user.Id, "لطفا یک مقدار عددی معتبر وارد کنید.",
                                        replyToMessageId: message.MessageId, replyMarkup: MarkupKeyboards.Cancel());
                                }

                                break;
                            case "usage":
                                if (message.Text.Fa2En().IsNumber())
                                {
                                    discount.UsageLimitation = int.Parse(message.Text.Fa2En());
                                    _uw.DiscountRepository.Update(discount);
                                    await _bot.SendTextMessageAsync(user.Id,
                                        "تعداد محدودیت استفاده کدتخفیف با موفقیت ویرایش شد.✅",
                                        replyToMessageId: message.MessageId,
                                        replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                                    await _bot.DeleteMessageAsync(user.Id, int.Parse(step.Split("*")[3]));
                                    await _bot.AddNewDiscount(user.Id, discount);
                                }
                                else
                                {
                                    await _bot.SendTextMessageAsync(user.Id, "لطفا یک مقدار عددی معتبر وارد کنید.",
                                        replyToMessageId: message.MessageId, replyMarkup: MarkupKeyboards.Cancel());
                                }

                                break;
                            default:
                                break;
                        }
                }

                break;
            default:
                break;
        }
    }
}