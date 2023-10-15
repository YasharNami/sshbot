using SSHVpnBot.Components.Base;
using SSHVpnBot.Components.Subscribers;
using SSHVpnBot.Repositories.Uw;
using SSHVpnBot.Telegram.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Components.AccountReports.Handlers;

public class AccountReportMessageHandler : MessageHandler
{
    public AccountReportMessageHandler(ITelegramBotClient _bot, Update update, IUnitOfWork _uw,
        Subscriber subscriber, CancellationToken cancellationToken = default)
        :base(_bot,update,_uw,subscriber)
    {
        
    }
    public override async Task MessageHandlerAsync()
    {
        switch (message.Type)
        {
            case MessageType.Text:
                if (step.StartsWith("reply*"))
                {
                    var report = await _uw.AccountReportRepository.GetReportByCodeAsync(step.Split("*")[1]);
                    if (report is not null)
                    {
                        var account = await _uw.AccountRepository.GetByAccountCode(report.AccountCode);
                        if (account is not null)
                        {
                            report.State = ReportState.Answered;
                            report.LastModifiedDate = DateTime.Now;
                            report.AnsweredBy = user.Id;
                            _uw.AccountReportRepository.Update(report);
                            var server = await _uw.ServerRepository.GetServerByCode(account.ServerCode);
                            await _bot.SendTextMessageAsync(report.UserId,
                                $"💌 اطلاعیه جدید دریافت شد.\n\n" +
                                $"در پاسخ به گزارش کندی روی سرور :\n" +
                                $"🔗 <code>{server.Domain}</code>\n\n" +
                                $"🔖 <code>{account.UserName}</code>\n\n" +
                                $"متن اطلاعیه :\n" +
                                $"{message.Text}", ParseMode.Html);

                            await _bot.SendTextMessageAsync(user.Id, "پاسخ شما به گزارش کندی با موفقیت ارسال شد.✅",
                                replyToMessageId: message.MessageId,
                                replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                            _uw.SubscriberRepository.ChangeStep(user.Id, "none");
                        }
                        else
                        {
                            await _bot.SendTextMessageAsync(user.Id, "اشتراک مورد نظر یافت نشد.",
                                replyToMessageId: message.MessageId);
                        }
                    }
                    else
                    {
                        await _bot.SendTextMessageAsync(user.Id, "گزارش مورد نظر یافت نشد.");
                    }
                }

                break;
            default:
                break;
        }
    }
}