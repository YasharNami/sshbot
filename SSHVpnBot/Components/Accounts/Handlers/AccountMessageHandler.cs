using ConnectBashBot.Commons;
using SSHVpnBot.Components.Accounts.Keyboards;
using SSHVpnBot.Components.Base;
using SSHVpnBot.Components.Subscribers;
using SSHVpnBot.Repositories.Uw;
using SSHVpnBot.Telegram.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Components.Accounts.Handlers;

public class AccountMessageHandler : MessageHandler
{
    public AccountMessageHandler(ITelegramBotClient _bot, Update update, IUnitOfWork _uw,
        Subscriber subscriber, CancellationToken cancellationToken = default)
        :base(_bot,update,_uw,subscriber)
    {
        
    }
    public override async Task MessageHandlerAsync()
    {
        switch (message.Type)
        {
            case MessageType.Text:
                if (step.StartsWith("note*"))
                {
                    if (message.Text.Length < 50 && message.Text.Length > 4)
                    {
                        var account = await _uw.AccountRepository.GetByAccountCode(step.Replace("note*", ""));
                        if (account is null)
                        {
                            await _bot.SendTextMessageAsync(user.Id, "اشتراک مور نظر یافت نشد.",
                                replyToMessageId: message.MessageId,
                                replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                        }
                        else
                        {
                            account.Note = message.Text.En2Fa();
                            _uw.AccountRepository.Update(account);
                            await _bot.SendTextMessageAsync(user.Id, $"یادداشت سفارش با موفقیت ویرایش شد.✅"
                                , replyToMessageId: message.MessageId,
                                replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                            _uw.SubscriberRepository.ChangeStep(user.Id, "none");
                        }
                    }
                    else
                    {
                        await _bot.SendTextMessageAsync(user.Id,
                            $"حداقل طول یادداشت ۵ کاراکتر و جداکثر طول آن می بایست ۵۰ کاراکتر باشد.\n" +
                            $"یادداشت دیگری وارد کنید :",
                            replyMarkup: MarkupKeyboards.Cancel());
                    }
                }
                else if (step.Equals("sendquery"))
                {
                    var query = message.Text.Trim();
                    if (query.Length > 3)
                    {
                        var accounts = await _uw.AccountRepository.GetByAccountNote(user.Id, query);
                        var services = _uw.ServiceRepository.GetAll().Where(s => !s.IsRemoved).ToList();
                        var colleague = await _uw.ColleagueRepository.GetByChatId(user.Id);
                        if (accounts.Count != 0)
                            await _bot.SendTextMessageAsync(user.Id,
                                $".\n" +
                                $"🔻 نتایج جستجوی شما بر اساس یادداشت :\n\n" +
                                $"🔍 عبارت جستجو شده : <b>{query}</b>\n\n" +
                                $"🌀 اشتراک مورد نظر خود را جهت بررسی انتخاب کنید :\n" +
                                $".",
                                ParseMode.Html,
                                replyMarkup: AccountKeyboards.AccountSearchByNoteResults(accounts,
                                    services, colleague.Tag.HasValue() ? colleague.Tag : ""));
                        else
                            await _bot.SendTextMessageAsync(user.Id, $"نتیجه ای یافت نشد.✖️"
                                , replyToMessageId: message.MessageId);
                    }
                    else
                    {
                        await _bot.SendTextMessageAsync(user.Id, "حداقل ۴ کاراکتر جهت جستجو با یادداشت وارد کنید.",
                            replyToMessageId: message.MessageId, replyMarkup: MarkupKeyboards.Cancel());
                    }
                }
                else if (step.Equals("senduid"))
                {
                      Task.Run(async () =>
                        {
                            var account = await _uw.AccountRepository.GetByAccountCode(message.Text.Fa2En().Trim());
                            var msg = await _bot.SendTextMessageAsync(user.Id, "در حال دریافت اطلاعات..⌛️",
                                replyToMessageId: message.MessageId,
                                replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                            if (account is not null)
                            {
                                var server = await _uw.ServerRepository.GetServerByCode(account.ServerCode);
                                if (server!.IsActive)
                                {
                                    var users = await _uw.PanelService.GetAllUsersAsync(server);
                                    var client = users.FirstOrDefault(s => s.Username.Equals(account.AccountCode.ToLower()));
                                    if (client is not null)
                                    {
                                        var service = await _uw.ServiceRepository.GetServiceByCode(account.ServiceCode);
                                        if (service is not null)
                                        {
                                            await _bot.SellerAccountInfo(_uw, user.Id, server, account,
                                                client.Traffics[0]);
                                        }
                                        else await _bot.SendTextMessageAsync(user.Id,"سرویس اشتراک یافت نشد.",replyToMessageId:message.MessageId);
                                    }
                                    else await _bot.SendTextMessageAsync(user.Id,"اشتراک یافت نشد.",replyToMessageId:message.MessageId);
                                }
                                else
                                {
                                    await _bot.SendTextMessageAsync(user.Id,
                                        $"سرور شناسه شما غیرفعال شده است.\n" +
                                        $"جهت پیگیری به پشتیبانی مراجعه نمایید.",
                                        replyToMessageId: message.MessageId,
                                        replyMarkup: MarkupKeyboards.Cancel());
                                }
                            }
                            else
                            {
                                await _bot.SendTextMessageAsync(user.Id,
                                    $".\n" +
                                    $"شناسه کانفیگ مورد نظر یافت نشد.\n" +
                                    $"لطفا اطلاعات ورودی را بررسی نمائید.🧐",
                                    replyToMessageId: message.MessageId,
                                    replyMarkup: MarkupKeyboards.Cancel());
                            }
                        });
                }

                break;
            default:
                break;
        }
    }
}