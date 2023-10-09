using ConnectBashBot.Commons;
using SSHVpnBot.Components.Base;
using SSHVpnBot.Components.Subscribers;
using SSHVpnBot.Repositories.Uw;
using SSHVpnBot.Telegram.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Components.Servers.Handlers;

public class ServerMessageHandler : MessageHandler
{
    public ServerMessageHandler(ITelegramBotClient _bot, Update update, IUnitOfWork _uw,
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
                    var server = await _uw.ServerRepository.GetServerByCode(step.Split("*")[1]);
                    var property = step.Split("*")[2];

                    switch (property)
                    {
                        case "url":
                            if (_uw.ServerRepository.AnyByUrl(server.Code, message.Text.Fa2En()))
                            {
                                await _bot.SendTextMessageAsync(user.Id,
                                    "آدرس سرور وارد شده ثبت شده است.",
                                    replyToMessageId: message.MessageId);
                            }
                            else
                            {
                                server.Url = message.Text.Fa2En();
                                await _bot.SendTextMessageAsync(user.Id, "آدرس سرور با موفقیت ویرایش شد ✅",
                                    replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                                _uw.ServerRepository.Update(server);
                                _uw.SubscriberRepository.ChangeStep(user.Id, $"none");
                            }

                            break;
                        case "username":
                            server.Username = message.Text.Fa2En();
                            await _bot.SendTextMessageAsync(user.Id, "نام کاربری سرور با موفقیت ویرایش شد ✅",
                                replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                            _uw.ServerRepository.Update(server);
                            _uw.SubscriberRepository.ChangeStep(user.Id, $"none");
                            break;
                        case "note":
                            server.Note = message.Text;
                            await _bot.SendTextMessageAsync(user.Id, "یادداشت سرور با موفقیت ویرایش شد ✅",
                                replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                            _uw.ServerRepository.Update(server);
                            _uw.SubscriberRepository.ChangeStep(user.Id, $"none");
                            break;
                        case "password":
                            server.Password = message.Text.Fa2En().Encrypt();
                            await _bot.SendTextMessageAsync(user.Id, "رمز عبور سرور با موفقیت ویرایش شد ✅",
                                replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                            _uw.ServerRepository.Update(server);
                            _uw.SubscriberRepository.ChangeStep(user.Id, $"none");
                            break;
                        case "sshpassword":
                            server.SSHPassword = message.Text.Fa2En().Encrypt();
                            await _bot.SendTextMessageAsync(user.Id, $"رمز عبور ریموت با موفقیت ویرایش شد.✅",
                                replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                            _uw.ServerRepository.Update(server);
                            _uw.SubscriberRepository.ChangeStep(user.Id, $"none");
                            break;
                        case "capacity":
                            server.Capacity = int.Parse(message.Text.Fa2En());
                            await _bot.SendTextMessageAsync(user.Id, "ظرفیت سرور با موفقیت ویرایش شد ✅",
                                replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                            _uw.ServerRepository.Update(server);
                            _uw.SubscriberRepository.ChangeStep(user.Id, $"none");
                            break;
                        case "domain":
                            if (_uw.ServerRepository.AnyByDomain(server.Code, message.Text.Fa2En()))
                            {
                                await _bot.SendTextMessageAsync(user.Id,
                                    "آدرس دامنه وارد شده ثبت شده است.",
                                    replyToMessageId: message.MessageId);
                            }
                            else
                            {
                                server.Domain = message.Text.Fa2En();
                                await _bot.SendTextMessageAsync(user.Id, "آدرس دامنه سرور با موفقیت ویرایش شد ✅",
                                    replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                                _uw.ServerRepository.Update(server);
                                _uw.SubscriberRepository.ChangeStep(user.Id, $"none");
                            }

                            break;
                        default:
                            break;
                    }

                    await _bot.DeleteMessageAsync(user.Id, int.Parse(step.Split("*")[3]));
                    await _bot.AddNewServer(_uw, user.Id, server);
                }
                else if (step.StartsWith("owner*"))
                {
                    var server = await _uw.ServerRepository.GetServerByCode(step.Split("*")[1]);
                    if (server is not null)
                    {
                        if (message.Text.IsNumber())
                        {
                            var colleague = await _uw.ColleagueRepository.GetByChatId(long.Parse(message.Text));
                            if (colleague is not null)
                            {
                                //server.OwnerId = colleague.UserId;
                                _uw.ServerRepository.Update(server);
                                await _bot.SendTextMessageAsync(user.Id,
                                    "سرور با موفقیت به همکار مورد نظر اختصاص داده شد.✅",
                                    replyToMessageId: message.MessageId,
                                    replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                                await _bot.DeleteMessageAsync(user.Id, int.Parse(step.Split("*")[2]));
                                await _bot.AddNewServer(_uw, user.Id, server);
                                _uw.SubscriberRepository.ChangeStep(user.Id, "none");
                            }
                            else
                            {
                                await _bot.SendTextMessageAsync(user.Id, "لطفا یک شناسه کاربری معتبر وارد کنید.",
                                    replyToMessageId: message.MessageId,
                                    replyMarkup: MarkupKeyboards.Cancel());
                            }
                        }
                        else
                        {
                            await _bot.SendTextMessageAsync(user.Id, "لطفا یک شناسه کاربری معتبر وارد کنید.",
                                replyToMessageId: message.MessageId,
                                replyMarkup: MarkupKeyboards.Cancel());
                        }
                    }
                    else
                    {
                        await _bot.SendTextMessageAsync(user.Id, "سرور مور نظر یافت نشد.");
                    }
                }

                break;
            default:
                break;
        }
    }
}