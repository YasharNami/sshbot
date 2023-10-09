using ConnectBashBot.Commons;
using SSHVpnBot.Components.Base;
using SSHVpnBot.Components.Subscribers;
using SSHVpnBot.Repositories.Uw;
using SSHVpnBot.Telegram.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Components.Services.Handlers;

public class ServiceMessageHandler : MessageHandler
{
    public ServiceMessageHandler(ITelegramBotClient _bot, Update update, IUnitOfWork _uw,
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
                    var service = await _uw.ServiceRepository.GetServiceByCode(step.Split("*")[1]);
                    var property = step.Split("*")[2];

                    switch (property)
                    {
                        case "title":
                            service.Title = message.Text;
                            await _bot.SendTextMessageAsync(user.Id, "نام سرویس با موفقیت ویرایش شد ✅",
                                replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                            _uw.ServiceRepository.Update(service);
                            _uw.SubscriberRepository.ChangeStep(user.Id, $"none");

                            break;
                        case "description":
                            service.Description = message.Text;
                            await _bot.SendTextMessageAsync(user.Id, "توضیحات سرویس با موفقیت ویرایش شد ✅",
                                replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                            _uw.ServiceRepository.Update(service);
                            _uw.SubscriberRepository.ChangeStep(user.Id, $"none");
                            break;
                        case "price":
                            service.Price = decimal.Parse(message.Text.Fa2En());
                            if (service.SellerPrice is 0)
                                service.SellerPrice = decimal.Parse(message.Text.Fa2En());
                            await _bot.SendTextMessageAsync(user.Id, "قیمت سرویس با موفقیت ویرایش شد ✅",
                                replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                            _uw.ServiceRepository.Update(service);
                            _uw.SubscriberRepository.ChangeStep(user.Id, $"none");
                            break;
                        case "duration":
                            service.Duration = int.Parse(message.Text.Fa2En());
                            await _bot.SendTextMessageAsync(user.Id, "مدت سرویس با موفقیت ویرایش شد ✅",
                                replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                            _uw.ServiceRepository.Update(service);
                            _uw.SubscriberRepository.ChangeStep(user.Id, $"none");
                            break;
                        case "traffic":
                            service.Traffic = int.Parse(message.Text.Fa2En());
                            await _bot.SendTextMessageAsync(user.Id, "حجم سرویس با موفقیت ویرایش شد ✅",
                                replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                            _uw.ServiceRepository.Update(service);
                            _uw.SubscriberRepository.ChangeStep(user.Id, $"none");
                            break;
                        case "userlimit":
                            service.UserLimit = int.Parse(message.Text.Fa2En());
                            await _bot.SendTextMessageAsync(user.Id, "تعداد کاربر سرویس با موفقیت ویرایش شد ✅",
                                replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                            _uw.ServiceRepository.Update(service);
                            _uw.SubscriberRepository.ChangeStep(user.Id, $"none");
                            break;
                        case "sellerprice":
                            if (message.Text!.Fa2En().IsNumber())
                            {
                                service!.SellerPrice = int.Parse(message.Text!.Fa2En());
                                _uw.ServiceRepository.Update(service);
                                await _bot.SendTextMessageAsync(user.Id, "قیمت پایه همکاری سرویس با موفقیت ویرایش شد.✅",
                                    replyToMessageId: message.MessageId, 
                                    replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                                _uw.SubscriberRepository.ChangeStep(user.Id, "none");
                            }
                            else
                                await _bot.SendTextMessageAsync(user.Id, $"لطفا یک قیمت صحیح وارد کنید.", replyToMessageId: message.MessageId);
                            break;
                        default:
                            break;
                    }

                    await _bot.DeleteMessageAsync(user.Id, int.Parse(step.Split("*")[3]));
                    await _bot.AddNewService(_uw, user.Id, service);
                }
                break;
            default:
                break;
        }
    }
}