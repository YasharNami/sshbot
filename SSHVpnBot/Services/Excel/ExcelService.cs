using System.Globalization;
using System.Text;
using ConnectBashBot.Commons;
using ConnectBashBot.Telegram.Handlers;
using CsvHelper;
using SSHVpnBot.Components.Servers;
using SSHVpnBot.Components.Transactions;
using SSHVpnBot.Services.Excel.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using File = System.IO.File;

namespace SSHVpnBot.Services.Excel;

public class ExcelService : IExcelService
{
    public async void SubscribersToCsv(IEnumerable<SubscriberReportModel> data, ITelegramBotClient bot,
        long group_id, int message_id)
    {
        var path = $"reports/{Guid.NewGuid()}-subscribers.csv";
        using (var mem = new MemoryStream())
        using (var writer = new StreamWriter(mem))
        using (var csvWriter = new CsvWriter(writer, CultureInfo.CurrentCulture))
        {
            using (var sr = new StreamWriter(path, false, Encoding.UTF8))
            {
                using (var csv = new CsvWriter(sr, CultureInfo.CurrentCulture))
                {
                    csvWriter.WriteField("User Id");
                    csvWriter.WriteField("FullName");
                    csvWriter.WriteField("Username");
                    csvWriter.WriteField("Role");
                    csvWriter.WriteField("Balance");
                    csvWriter.WriteField("Orders");
                    csvWriter.WriteField("Accounts");
                    csvWriter.WriteField("State");
                    csvWriter.WriteField("JoinedOn");
                    csvWriter.NextRecord();

                    foreach (var item in data)
                    {
                        csvWriter.WriteField(item.UserId);
                        csvWriter.WriteField(item.FullName);
                        csvWriter.WriteField(item.Username);
                        csvWriter.WriteField(item.Role);
                        csvWriter.WriteField(item.Balance);
                        csvWriter.WriteField(item.Orders);
                        csvWriter.WriteField(item.Accounts);
                        csvWriter.WriteField(item.isActive);
                        csvWriter.WriteField(item.JoinedOn);
                        csvWriter.NextRecord();
                    }

                    writer.Flush();
                    csv.WriteRecords(data);
                }
            }

            using (var fs = File.Open(path, FileMode.Open))
            {
                await bot.DeleteMessageAsync(group_id, message_id);
                await bot.SendDocumentAsync(group_id,
                    new InputOnlineFile(fs, $"subscribers.csv"),
                    caption: $"🗂 گزارش لیست کاربران\n\n" +
                             $"🕧 {DateTime.Now.ConvertToPersianCalendar().En2Fa()} ساعت {DateTime.Now.ToString("HH:mm").En2Fa()}",
                    parseMode: ParseMode.Html);
            }
        }
    }

    public async void ColleaguesToCsv(IEnumerable<ColleagueReportModel> data, ITelegramBotClient bot,
        long group_id, int message_id)
    {
        var path = $"reports/{Guid.NewGuid()}-sellers.csv";
        using (var mem = new MemoryStream())
        using (var writer = new StreamWriter(mem))
        using (var csvWriter = new CsvWriter(writer, CultureInfo.CurrentCulture))
        {
            using (var sr = new StreamWriter(path, false, Encoding.UTF8))
            {
                using (var csv = new CsvWriter(sr, CultureInfo.CurrentCulture))
                {
                    csvWriter.WriteField("User Id");
                    csvWriter.WriteField("Tag");
                    csvWriter.WriteField("TotalPayments");
                    csvWriter.WriteField("Accounts");
                    csvWriter.WriteField("Balance");
                    csvWriter.WriteField("Orders");
                    csvWriter.WriteField("JoinedOn");
                    csvWriter.NextRecord();

                    foreach (var item in data)
                    {
                        csvWriter.WriteField(item.UserId);
                        csvWriter.WriteField(item.Tag);
                        csvWriter.WriteField(item.TotalPayments);
                        csvWriter.WriteField(item.Accounts);
                        csvWriter.WriteField(item.Balance);
                        csvWriter.WriteField(item.Orders);
                        csvWriter.WriteField(item.JoinedOn);

                        csvWriter.NextRecord();
                    }

                    writer.Flush();
                    csv.WriteRecords(data);
                }
            }

            using (var fs = File.Open(path, FileMode.Open))
            {
                await bot.DeleteMessageAsync(group_id, message_id);
                await bot.SendDocumentAsync(group_id,
                    new InputOnlineFile(fs, $"sellers.csv"),
                    caption: $"🗂 گزارش لیست همکاران\n\n" +
                             $"🕧 {DateTime.Now.ConvertToPersianCalendar().En2Fa()} ساعت {DateTime.Now.ToString("HH:mm").En2Fa()}",
                    parseMode: ParseMode.Html);
            }
        }
    }

    public async Task ServerReportsToCsv(Server server, IEnumerable<ServerReportsReportModel> data,
        ITelegramBotClient bot)
    {
        var path = $"reports/{Guid.NewGuid()}-serverreports.csv";
        using (var mem = new MemoryStream())
        using (var writer = new StreamWriter(mem))
        using (var csvWriter = new CsvWriter(writer, CultureInfo.CurrentCulture))
        {
            using (var sr = new StreamWriter(path, false, Encoding.UTF8))
            {
                using (var csv = new CsvWriter(sr, CultureInfo.CurrentCulture))
                {
                    csvWriter.WriteField("User Id");
                    csvWriter.WriteField("Client Id");
                    csvWriter.WriteField("Account State");
                    csvWriter.WriteField("Url");
                    csvWriter.WriteField("Server");
                    csvWriter.WriteField("Operator");
                    csvWriter.WriteField("Description");
                    csvWriter.WriteField("Last Modified Date");
                    csvWriter.WriteField("Type");
                    csvWriter.WriteField("State");
                    csvWriter.WriteField("AnsweredBy");
                    csvWriter.WriteField("Created On");

                    csvWriter.NextRecord();

                    foreach (var item in data)
                    {
                        csvWriter.WriteField(item.UserId);
                        csvWriter.WriteField(item.ClientId);
                        csvWriter.WriteField(item.AccountState);
                        csvWriter.WriteField(item.Url);
                        csvWriter.WriteField(item.Server);
                        csvWriter.WriteField(item.Operator);
                        csvWriter.WriteField(item.Description);
                        csvWriter.WriteField(item.LastModifiedDate);
                        csvWriter.WriteField(item.Type);
                        csvWriter.WriteField(item.State);
                        csvWriter.WriteField(item.AnsweredBy);
                        csvWriter.WriteField(item.CreatedOn);

                        csvWriter.NextRecord();
                    }

                    writer.Flush();
                    csv.WriteRecords(data);
                }
            }

            using (var fs = File.Open(path, FileMode.Open))
            {
                await bot.SendDocumentAsync(MainHandler._reportgroup,
                    new InputOnlineFile(fs, $"{server.Domain.Split(".")[0]}-reports.csv"),
                    caption: $"🗂 گزارش لیست گزارشات اختلال سرور\n\n" +
                             $"🌐 <b>{server.Domain}</b>\n" +
                             $"🕧 {DateTime.Now.ConvertToPersianCalendar().En2Fa()} ساعت {DateTime.Now.ToString("HH:mm").En2Fa()}",
                    parseMode: ParseMode.Html);
            }
        }
    }


    public async void TransactionsToCsv(IEnumerable<Transaction> data, ITelegramBotClient bot,
        long group_id)
    {
        var path = $"reports/{Guid.NewGuid()}-transactions.csv";
        using (var mem = new MemoryStream())
        using (var writer = new StreamWriter(mem))
        using (var csvWriter = new CsvWriter(writer, CultureInfo.CurrentCulture))
        {
            using (var sr = new StreamWriter(path, false, Encoding.UTF8))
            {
                using (var csv = new CsvWriter(sr, CultureInfo.CurrentCulture))
                {
                    csvWriter.WriteField("TransactionCode");
                    csvWriter.WriteField("Amount");
                    csvWriter.WriteField("Type");
                    csvWriter.WriteField("Date");
                    csvWriter.NextRecord();

                    foreach (var item in data)
                    {
                        csvWriter.WriteField(item.TransactionCode);
                        csvWriter.WriteField(item.Amount.ToIranCurrency());
                        csvWriter.WriteField(nameof(item.Type));
                        csvWriter.WriteField(item.CreatedOn.ToPersianDate() + " " + item.CreatedOn.ToString("HH:mm"));
                        csvWriter.NextRecord();
                    }

                    writer.Flush();
                    csv.WriteRecords(data);
                }
            }

            using (var fs = File.Open(path, FileMode.Open))
            {
                await bot.SendDocumentAsync(group_id,
                    new InputOnlineFile(fs, $"transactions-U{data.First().UserId}.csv"),
                    caption: $"🗂 گزارش لیست تراکنش ها\n" +
                             $"👤 #U{data.First().UserId}\n" +
                             $"📌 <b>{data.Count().En2Fa()} تراکنش</b>\n" +
                             $"💰 موجودی فعلی : <b>{data.Sum(s => s.Amount).ToIranCurrency().En2Fa()} تومان</b>\n\n" +
                             $"تا {DateTime.Now.ConvertToPersianCalendar().En2Fa()}" +
                             $"ساعت {DateTime.Now.ToString("HH:mm").En2Fa()}",
                    parseMode: ParseMode.Html);
            }
        }
    }
}