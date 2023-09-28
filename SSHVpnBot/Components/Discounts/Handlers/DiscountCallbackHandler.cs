using ConnectBashBot.Commons;
using ConnectBashBot.Telegram.Handlers;
using SSHVpnBot.Components.Base;
using SSHVpnBot.Components.Discounts.Keyboards;
using SSHVpnBot.Components.Subscribers;
using SSHVpnBot.Repositories.Uw;
using SSHVpnBot.Telegram;
using SSHVpnBot.Telegram.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Components.Discounts.Handlers;

public class DiscountCallbackHandler : QueryHandler
{
    public DiscountCallbackHandler(ITelegramBotClient _bot, Update update,
        IUnitOfWork _uw,
        Subscriber subscriber, CancellationToken cancellationToken = default)
        :base(_bot,update,_uw,subscriber)
    {
        
    }
    public override async Task QueryHandlerAsync()
    {
        if (data.Equals("discounts"))
        {
            await _bot.Choosed(callBackQuery);
            var discounts = _uw.DiscountRepository.GetAll().Where(s => !s.IsRemoved);
            foreach (var discount in discounts)
            {
                var orders = await _uw.OrderRepository.GetAllByDiscountNumber(discount.DiscountNumber);
                discount.UsedCount = orders;
            }

            await _bot.SendTextMessageAsync(groupId,
                $"<b>🎉 مدیریت کدهای تخفیف</b>\n\n" +
                $"🟢 کد های تخفیف فعال : <b>{discounts.Where(s => s.IsActive).ToList().Count.ToString().En2Fa()}</b>\n" +
                $"🔴 کد های تخفیف غیرفعال : <b>{discounts.Where(s => !s.IsActive).ToList().Count.ToString().En2Fa()}</b>\n\n" +
                $"🔖 قصد مدیریت کدام کد تخفیف را دارید؟",
                ParseMode.Html,
                replyMarkup: DiscountKeyboards.DiscountManagement(discounts));
        }
        else if (data.Equals("newdiscount"))
        {
            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                "اطلاعات کدتخفیف جدید جهت ویرایش برای شما ارسال شد.✔️", true);

            var discountNumber = Discount.GenerateNewDiscountNumber();
            var discount = new Discount()
            {
                DiscountNumber = discountNumber,
                IsActive = false,
                CreatedOn = DateTime.Now,
                ExpiredOn = DateTime.Now.AddDays(30),
                Type = DiscountType.Percent,
                Amount = 0
            };
            _uw.DiscountRepository.Add(discount);

            discount = await _uw.DiscountRepository.GetByDiscountNumberAsync(discount.DiscountNumber);
            await _bot.AddNewDiscount(user.Id, discount);
        }
        else if (data.StartsWith("discount*"))
        {
            var discount =
                await _uw.DiscountRepository.GetByDiscountNumberAsync(data.Replace("discount*", ""));
            if (discount is null)
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "کدتخفیف یافت نشد", true);

            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                "اطلاعات کدتخفیف مورد نظر جهت ویرایش برایتان ارسال شد.", true);

            await _bot.AddNewDiscount(user.Id, discount);
        }
        else if (data.StartsWith("update*"))
        {
            var discount = await _uw.DiscountRepository.GetByIdAsync(int.Parse(data.Split("*")[1]));
            if (discount is not null)
            {
                var section = data.Split("*")[2];
                switch (section)
                {
                    case "code":
                        await _bot.Choosed(callBackQuery);
                        await _bot.SendTextMessageAsync(user.Id, "🔖️ کد تخفیف را به انگلیسی وارد نمایید :"
                            , replyMarkup: MarkupKeyboards.Cancel());
                        _uw.SubscriberRepository.ChangeStep(user.Id,
                            $"{Constants.DiscountConstants}-update*{discount.Id}*code*{callBackQuery.Message.MessageId}");
                        break;
                    case "user":
                        await _bot.Choosed(callBackQuery);
                        await _bot.SendTextMessageAsync(user.Id,
                            "👤 شناسه کاربری کاربر مورد نظر را وارد نمایید :"
                            , replyMarkup: MarkupKeyboards.Cancel());
                        _uw.SubscriberRepository.ChangeStep(user.Id,
                            $"{Constants.DiscountConstants}-update*{discount.Id}*user*{callBackQuery.Message.MessageId}");
                        break;
                    case "maxpercentamount":
                        if (discount.Type.Equals(DiscountType.Percent))
                        {
                            await _bot.Choosed(callBackQuery);
                            await _bot.SendTextMessageAsync(user.Id,
                                "📍️ سقف میزان درصد را به تومان وارد نمایید :"
                                , replyMarkup: MarkupKeyboards.Cancel());
                            _uw.SubscriberRepository.ChangeStep(user.Id,
                                $"{Constants.DiscountConstants}-update*{discount.Id}*maxpercentamount*{callBackQuery.Message.MessageId}");
                        }
                        else
                        {
                            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                                "این قسمت برای کد های تخفیف درصدی می باشد.", true);
                        }

                        break;
                    case "amount":
                        await _bot.Choosed(callBackQuery);
                        await _bot.SendTextMessageAsync(user.Id,
                            "💲️ مقدار تخفیف را به صورت عددی وارد نمایید :",
                            replyMarkup: MarkupKeyboards.Cancel());
                        _uw.SubscriberRepository.ChangeStep(user.Id,
                            $"{Constants.DiscountConstants}-update*{discount.Id}*amount*{callBackQuery.Message.MessageId}");
                        break;
                    case "type":
                        await _bot.Choosed(callBackQuery);
                        await _bot.SendTextMessageAsync(user.Id,
                            "⌛ نوع کد تخفیف استفاده را انتخاب نمایید :",
                            replyMarkup: DiscountKeyboards.DiscountTypes(discount,
                                callBackQuery.Message.MessageId));
                        break;
                    case "service":
                        await _bot.Choosed(callBackQuery);
                        var services = await _uw.ServiceRepository.GetAllPublicsAsync();
                        await _bot.SendTextMessageAsync(user.Id,
                            "⌛ این کد تخفیف را به کدام سرویس اختصاص میدهید :",
                            replyMarkup: DiscountKeyboards.DiscountServices(discount, services,
                                callBackQuery.Message.MessageId));
                        break;
                    case "usage":
                        await _bot.Choosed(callBackQuery);
                        await _bot.SendTextMessageAsync(user.Id,
                            "👥️ محدودیت تعداد استفاده را به صورت عددی وارد نمایید :",
                            replyMarkup: MarkupKeyboards.Cancel());
                        _uw.SubscriberRepository.ChangeStep(user.Id,
                            $"{Constants.DiscountConstants}-update*{discount.Id}*usage*{callBackQuery.Message.MessageId}");
                        break;
                    case "expiredon":
                        await _bot.Choosed(callBackQuery);
                        await _bot.SendTextMessageAsync(user.Id, "⌛ تاریخ انقضا استفاده را انتخاب نمایید :",
                            replyMarkup: DiscountKeyboards.DiscountDurations(discount,
                                callBackQuery.Message.MessageId));
                        break;
                    case "remove":
                        await _bot.Choosed(callBackQuery);
                        await _bot.SendTextMessageAsync(groupId,
                            $"آیا از حذف کد تخفیف به شرح فوق اطمینان دارید؟\n\n",
                            ParseMode.Html,
                            replyToMessageId: callBackQuery.Message.MessageId,
                            replyMarkup:
                            DiscountKeyboards.RemoveDiscountConfirmation(discount));
                        break;
                    case "done":
                        if (discount.Code.HasValue() && discount.Amount != 0)
                        {
                            discount.IsActive = true;
                            _uw.DiscountRepository.Update(discount);
                            await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
                            await _bot.SendTextMessageAsync(MainHandler._managementgroup,
                                $"<b>کدتخفیف جدید به شرح زیر توسط {user.FirstName + " " + user.LastName} ویرایش/اضافه شد.✔️</b>\n\n" +
                                $"🔖 شناسه کدتخفیف : <code>#{discount.DiscountNumber}</code>\n" +
                                $"📌 کد : <b>{discount.Code}</b>\n" +
                                $"💬️ نوع کدتخفیف : <b>{discount.Type.ToDisplay()}</b>\n" +
                                $"💲 مقدار تخفیف : {(discount.Type == DiscountType.Amount ? discount.Amount.ToIranCurrency().En2Fa() + " تومان" : discount.Amount.En2Fa() + " درصد")}\n" +
                                $"⌛ تاریخ انقضا : <b>{(discount.ExpiredOn == default ? "بدون محدودیت" : discount.ExpiredOn.ToPersianDate().En2Fa())}</b>\n" +
                                $"👥 تعداد استفاده : <b>{(discount.UsageLimitation == 0 ? "بدون محدودیت" : discount.UsageLimitation.ToString().En2Fa() + " کاربر")}</b>\n" +
                                $"📍 وضعیت : <b>{(discount.IsActive ? "فعال 🟢" : "غیرفعال 🔴")}</b>\n\n",
                                ParseMode.Html);
                        }
                        else
                        {
                            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                                "پیش از افزودن کدتخفیف اطلاعات موردنیاز را تکمیل نمایید.", true);
                        }

                        break;
                    default:
                        break;
                }
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "کد تخفیف مورد نظر یافت نشد.", true);
            }
        }
        else if (data.StartsWith("service*"))
        {
            var discount = await _uw.DiscountRepository.GetByCodeAsync(data.Split("*")[1]);
            if (discount is not null)
            {
                var service = await _uw.ServiceRepository.GetServiceByCode(data.Split("*")[2]);
                if (service is not null)
                {
                    await _bot.Choosed(callBackQuery);
                    discount.ServiceCode = service.Code;
                    _uw.DiscountRepository.Update(discount);
                    await _bot.DeleteMessageAsync(user.Id, int.Parse(data.Split("*")[3]));
                    await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
                    await _bot.AddNewDiscount(user.Id, discount);
                }
                else
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, $"سرویس مورد نظر یافت نشد",
                        true);
                }
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, $"کد تخفیف یافت نشد", true);
            }
        }
        else if (data.StartsWith("remove*"))
        {
            var discount = await _uw.DiscountRepository.GetByIdAsync(int.Parse(data.Split("*")[2]));
            if (discount is not null)
            {
                if (data.Split("*")[1].Equals("approve"))
                {
                    discount.IsRemoved = true;
                    _uw.DiscountRepository.Update(discount);
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                        "کد تخفیف با موفقیت حذف شد.✅", true);
                    await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
                    _bot.DiscountRemovedReport(user.Id, discount, user);
                }
                else
                {
                    await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
                }
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "کد تخفیف مورد نظر یافت نشد.", true);
            }
        }
        else if (data.StartsWith("duration*"))
        {
            var discount = await _uw.DiscountRepository.GetByIdAsync(int.Parse(data.Split("*")[1]));
            if (discount is not null)
            {
                if (!data.Split("*")[2].Equals("0"))
                    discount.ExpiredOn = discount.CreatedOn.AddDays(int.Parse(data.Split("*")[2]));

                _uw.DiscountRepository.Update(discount);
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                    "تاریخ انقضا کد تخفیف با موفقیت انتخاب شد.✅");
                await _bot.DeleteMessageAsync(user.Id, int.Parse(data.Split("*")[3]));
                await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);

                await _bot.AddNewDiscount(user.Id, discount);
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "کد تخفیف مورد نظر یافت نشد.", true);
            }
        }
        else if (data.StartsWith("type*"))
        {
            var discount = await _uw.DiscountRepository.GetByIdAsync(int.Parse(data.Split("*")[1]));
            if (discount is not null)
            {
                discount.Type = data.Split("*")[2] == "percent"
                    ? DiscountType.Percent
                    : DiscountType.Amount;
                _uw.DiscountRepository.Update(discount);
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                    "نوع کد تخفیف با موفقیت انتخاب شد.✅");
                await _bot.DeleteMessageAsync(user.Id, int.Parse(data.Split("*")[3]));
                await _bot.AddNewDiscount(user.Id, discount);
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "کد تخفیف مورد نظر یافت نشد.", true);
            }
        }
    }
}