using SSHVpnBot.Components.Servers;
using SSHVpnBot.Components.Transactions;
using SSHVpnBot.Services.Excel.Models;
using Telegram.Bot;

namespace SSHVpnBot.Services.Excel;

public interface IExcelService
{
    void SubscribersToCsv(IEnumerable<SubscriberReportModel> data, ITelegramBotClient bot,
        long group_id, int message_id);

    void ColleaguesToCsv(IEnumerable<ColleagueReportModel> data, ITelegramBotClient bot,
        long group_id, int message_id);

    Task ServerReportsToCsv(Server server, IEnumerable<ServerReportsReportModel> data, ITelegramBotClient bot);

    void TransactionsToCsv(IEnumerable<Transaction> data, ITelegramBotClient bot,
        long group_id);
}