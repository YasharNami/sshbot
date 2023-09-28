using Telegram.Bot.Types.ReplyMarkups;

namespace SSHVpnBot.Components.AccountReports.Keyboards;

public class AccountReportKeyboards
{
    public static IReplyMarkup ReplyToReport(string code)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData($"در حال بررسی 🌀",
                    $"{Constants.AccountReportConstants}-repairing*{code}")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"ثبت پاسخ 💬",
                    $"{Constants.AccountReportConstants}-reply*{code}"),
                InlineKeyboardButton.WithCallbackData($"اتمام بررسی 📮",
                    $"{Constants.AccountReportConstants}-checked*{code}")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"برطرف شد 🟢", $"{Constants.AccountReportConstants}-solved*{code}")
            }
        });
    }

    public static IReplyMarkup Operators(string accountCode, AccountReportType type)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData($"🌐 ایرانسل",
                    $"{Constants.AccountReportConstants}-operator*{(int)AccountOperator.Irancell}*{accountCode}*{(int)type}")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"🌐 همراه اول",
                    $"{Constants.AccountReportConstants}-operator*{(int)AccountOperator.MCI}*{accountCode}*{(int)type}")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"🌐 رایتل",
                    $"{Constants.AccountReportConstants}-operator*{(int)AccountOperator.Rightel}*{accountCode}*{(int)type}")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"🌐 وای فای",
                    $"{Constants.AccountReportConstants}-operator*{(int)AccountOperator.WIFI}*{accountCode}*{(int)type}")
            }
        });
    }
}