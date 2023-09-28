using ConnectBashBot.Commons;
using ConnectBashBot.Telegram.Handlers;
using SSHVpnBot.Components.AccountReports.Keyboards;
using SSHVpnBot.Components.Accounts;
using SSHVpnBot.Components.Base;
using SSHVpnBot.Components.Services;
using SSHVpnBot.Components.Subscribers;
using SSHVpnBot.Repositories.Uw;
using SSHVpnBot.Telegram;
using SSHVpnBot.Telegram.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Components.AccountReports.Handlers;


public class AccountReportCallbackHandler : QueryHandler
{
    public AccountReportCallbackHandler(ITelegramBotClient _bot, Update update,
        IUnitOfWork _uw,
        Subscriber subscriber, CancellationToken cancellationToken = default)
        : base(_bot, update, _uw, subscriber)
    {
    }

    public override async Task QueryHandlerAsync()
    {
        if (data.StartsWith("reportdisconect*"))
        {
            var account = await _uw.AccountRepository.GetByAccountCode(data.Split("*")[1]);
            if (account.State == AccountState.Active)
            {
                if (account.Type != AccountType.Check)
                    await _bot.SendTextMessageAsync(user.Id,
                        $".\n\n" +
                        $"🌐 اپراتور خود را جهت گزارش قطعی انتخاب کنید :",
                        ParseMode.Html,
                        replyMarkup: AccountReportKeyboards.Operators(account.AccountCode,
                            AccountReportType.Disconnection));
                else
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, $"اشتراک تست پشتیبانی نمی شود.", true);
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                    $"وضعیت سرویس در حالت {account.State.ToDisplay()} قرار دارد.", true);
            }
        }
        else if (data.StartsWith("reportlowspeed*"))
        {
            var account = await _uw.AccountRepository.GetByAccountCode(data.Split("*")[1]);
            if (account.State == AccountState.Active)
            {
                if (account.Type != AccountType.Check)
                {
                    if (!await _uw.AccountReportRepository.AnyOpenReportByAccountCode(account.AccountCode))
                        await _bot.SendTextMessageAsync(user.Id,
                            $".\n\n" +
                            $"🌐 اپراتور خود را جهت گزارش کندی انتخاب کنید :",
                            ParseMode.Html,
                            replyMarkup: AccountReportKeyboards.Operators(account.AccountCode,
                                AccountReportType.LowSpeed));
                }
                else
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, $"اشتراک تست پشتیبانی نمی شود.", true);
                }
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                    $"وضعیت سرویس در حالت {account.State.ToDisplay()} قرار دارد.", true);
            }
        }
        else if (data.StartsWith("solved*"))
        {
            var report = await _uw.AccountReportRepository.GetReportByCodeAsync(data.Split("*")[1]);
            if (report is not null)
            {
                report.State = ReportState.Fixed;
                report.LastModifiedDate = DateTime.Now;
                _uw.AccountReportRepository.Update(report);
                await _bot.Choosed(callBackQuery);
                await _bot.EditMessageTextAsync(groupId, callBackQuery.Message.MessageId,
                    callBackQuery.Message.Text, ParseMode.Html);
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "گزارش مورد نظر یافت نشد.", true);
            }
        }
        else if (data.StartsWith("operator*"))
        {
            var @operator = int.Parse(data.Split("*")[1]);
            var account = await _uw.AccountRepository.GetByAccountCode(data.Split("*")[2]);
            var type = (AccountReportType)int.Parse(data.Split("*")[3]);
            if (account is not null)
            {
                var server = await _uw.ServerRepository.GetServerByCode(account.ServerCode);
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "اشتراک مورد نظر یافت نشد.", true);
            }
        }
        else if (data.StartsWith("reply*"))
        {
            var report = await _uw.AccountReportRepository.GetReportByCodeAsync(data.Split("*")[1]);
            if (report is not null)
            {
                var account = await _uw.AccountRepository.GetByAccountCode(report.AccountCode);
                if (account is not null)
                {
                    var server = await _uw.ServerRepository.GetServerByCode(account.ServerCode);
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "پاسخ خود را به ربات ارسال کنید", true);
                    await _bot.SendTextMessageAsync(user.Id,
                        $"پاسخ خود را در رابطه با گزارش اشتراک\n\n" +
                        $"🔖 <code>#{account.ClientId}</code>\n" +
                        $"روی سرور :\n" +
                        $"🔗 <code>{server.Domain}:{account.Port}</code>\n\n" +
                        $"ارسال نمایید :",
                        ParseMode.Html,
                        replyMarkup: MarkupKeyboards.Cancel());
                    _uw.SubscriberRepository.ChangeStep(user.Id, $"{Constants.AccountReportConstants}-{data}");
                }
                else
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "اشتراک مورد نظر یافت نشد.", true);
                }
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "گزارش مورد نظر یافت نشد.", true);
            }
        }
        else if (data.StartsWith("repairing*"))
        {
            var report = await _uw.AccountReportRepository.GetReportByCodeAsync(data.Split("*")[1]);
            if (report is not null)
            {
                var account = await _uw.AccountRepository.GetByAccountCode(report.AccountCode);
                if (account is not null)
                {
                    report.State = ReportState.Checking;
                    report.LastModifiedDate = DateTime.Now;
                    _uw.AccountReportRepository.Update(report);
                    await _bot.SendTextMessageAsync(account.UserId,
                        $"سلام وقت بخیر، گزارش شما دریافت شد.✅\n\n" +
                        $"مشکل اتصال شما توسط کارشناسان در حال برسی می باشد، بزودی نتیجه از همین طریق به شما اعلام میگردد.🤍\n" +
                        $"@connect_bash\n\n" +
                        $"شناسه اشتراک شما :\n" +
                        $"🔗 <code>{account.ClientId}</code>\n\n" +
                        $"موفق باشید 🌹",
                        ParseMode.Html);

                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                        "اعلان بررسی مجدد اشتراک برای کاربر ارسال شد.", true);
                }

                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "اشتراک مورد نظر یافت نشد.", true);
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "گزارش مورد نظر یافت نشد.", true);
            }
        }
        else if (data.StartsWith("checked*"))
        {
            var report = await _uw.AccountReportRepository.GetReportByCodeAsync(data.Split("*")[1]);
            if (report is not null)
            {
                var account = await _uw.AccountRepository.GetByAccountCode(report.AccountCode);
                if (account is not null)
                {
                    report.State = ReportState.Checked;
                    report.LastModifiedDate = DateTime.Now;
                    _uw.AccountReportRepository.Update(report);
                    await _bot.SendTextMessageAsync(account.UserId,
                        $"سلام وقت بخیر گزارش شما بررسی شد.✅\n\n" +
                        $"لطفا مجددا بررسی کنید و در صورت پابرجا بودن مشکل در اکانت پشتیبانی درخدمت شما هستیم🤍\n" +
                        $"@connect_bash\n\n" +
                        $"شناسه اشتراک شما :\n" +
                        $"🔗 <code>{account.ClientId}</code>\n\n" +
                        $"موفق باشید 🌹",
                        ParseMode.Html,
                        replyMarkup: MarkupKeyboards.Cancel());

                    await _bot.EditMessageTextAsync(MainHandler._reportgroup, callBackQuery.Message.MessageId,
                        callBackQuery.Message.Text);
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                        "اتمام بررسی و اعلان تست مجدد اشتراک برای کاربر ارسال شد.", true);
                }
                else
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "اشتراک مورد نظر یافت نشد.", true);
                }
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "گزارش مورد نظر یافت نشد.", true);
            }
        }
    }
}