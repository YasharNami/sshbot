using ConnectBashBot.Commons;
using SSHVpnBot.Components.Colleagues;
using SSHVpnBot.Components.Servers;
using SSHVpnBot.Components.Services;
using SSHVpnBot.Services.Panel.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace SSHVpnBot.Components.Accounts.Keyboards;

public class AccountKeyboards
{
    public static IReplyMarkup MineServices(List<Account> accounts, List<Service> services)
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();
        foreach (var account in accounts.Where(s => s.Type == AccountType.Normal && s.ServiceCode != "BYADMIN"))
            buttonLines.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData(
                    $"🔗 {account.UserName}",
                    $"{Constants.AccountConstants}-account*{account.AccountCode}")
            });

        if (accounts.Any(s => s.Type == AccountType.Check))
        {
            var account = accounts.FirstOrDefault(s => s.Type == AccountType.Check);
            buttonLines.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData(
                    $"🧪 اکانت تست ۳ ساعته ",
                    $"{Constants.AccountConstants}-account*{account.AccountCode}")
            });
        }

        return new InlineKeyboardMarkup(buttonLines);
    }

    public static InlineKeyboardMarkup SearchAccountByNote()
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData("🔍 جستجو بر اساس شناسه",
                    $"{Constants.AccountConstants}-searchbyuid")
            },
            new() { InlineKeyboardButton.WithCallbackData("🔙 بازگشت به منوی اصلی", "back*menu") }
        });
    }

    public static IReplyMarkup AccountSearchByNoteResults(List<Account> accounts, List<Service> services, string tag)
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();
        foreach (var account in accounts)
            buttonLines.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData(
                    $"🔗 {account.UserName} / 📝 {account.Note}",
                    $"{Constants.AccountConstants}-account*{account.AccountCode}")
            });

        return new InlineKeyboardMarkup(buttonLines);
    }

    public static InlineKeyboardMarkup SearchAccount()
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData("🔍 جستجو بر اساس یادداشت",
                    $"{Constants.AccountConstants}-searchbynote")
            },
            new() { InlineKeyboardButton.WithCallbackData("🔙 بازگشت به منوی اصلی", "back*menu") }
        });
    }

    public static IReplyMarkup ExtendConfigConfirmation(Server server, string code, int port)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData($"بله اطمینان دارم 👍",
                    $"{Constants.AccountConstants}-extconfg*approve*{code}*{server.Id}*{port}")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"خیر مطمین نیستم 👍",
                    $"{Constants.AccountConstants}-extconfg*decline*{code}*{server.Id}*{port}")
            }
        });
    }

    public static IReplyMarkup LearnConnection()
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithUrl($"راهنمای اتصال ℹ️", $"https://t.me/connectbash/309")
            }
        });
    }

    public static IReplyMarkup ExtendAccount(string accountCode)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData($"⌛️ تمدید سرویس",
                    $"{Constants.AccountConstants}-extend*{accountCode}")
            }
        });
    }

    public static IReplyMarkup RemoveConfigConfirmation(Server server, string accountCode)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData($"بله اطمینان دارم 👍",
                    $"{Constants.AccountConstants}-rmconf*{accountCode}*{server.Id}")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"خیر مطمین نیستم 👍", $"deleteme")
            }
        });
    }

    public static IReplyMarkup BlockonfigConfirmation(Server server, string clientId, int port)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData($"بله اطمینان دارم 👍",
                    $"{Constants.AccountConstants}-block*approve*{clientId}*{server.Id}*{port}")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"خیر مطمین نیستم 👍",
                    $"{Constants.AccountConstants}-block*decline*{clientId}*{server.Id}*{port}")
            }
        });
    }

    public static IReplyMarkup UnBlockonfigConfirmation(Server server, string clientId, int port)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData($"بله اطمینان دارم 👍",
                    $"{Constants.AccountConstants}-unblock*approve*{clientId}*{server.Id}*{port}")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"خیر مطمین نیستم 👍",
                    $"{Constants.AccountConstants}-unblock*decline*{clientId}*{server.Id}*{port}")
            }
        });
    }

    public static IReplyMarkup ReportAccount(Account account)
    {
        if (account.Type == AccountType.Check)
            return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
            {
                new()
                {
                    InlineKeyboardButton.WithCallbackData($"♻️ دریافت مجدد کانفیگ️️",
                        $"{Constants.AccountConstants}-reget*{account.AccountCode}")
                }
                // new()
                // {
                //     InlineKeyboardButton.WithCallbackData($"🐢 گزارش کندی و اختلال️",
                //         $"{Constants.AccountReportConstants}-reportlowspeed*{account.AccountCode}")
                // },
                // new()
                // {
                //     InlineKeyboardButton.WithCallbackData($"🔴 گزارش قطعی",
                //         $"{Constants.AccountReportConstants}-reportdisconect*{account.AccountCode}")
                // }
            });
        else
            return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
            {
                new()
                {
                    InlineKeyboardButton.WithCallbackData($"⌛️ تمدید سرویس",
                        $"{Constants.AccountConstants}-extend*{account.AccountCode}")
                }
                // new()
                // {
                //     InlineKeyboardButton.WithCallbackData($"🐢 گزارش کندی و اختلال️",
                //         $"{Constants.AccountReportConstants}-reportlowspeed*{account.AccountCode}")
                // },
                // new()
                // {
                //     InlineKeyboardButton.WithCallbackData($"🔴 گزارش قطعی",
                //         $"{Constants.AccountReportConstants}-reportdisconect*{account.AccountCode}")
                // }
            });
    }

    public static IReplyMarkup MineAccounts(List<Account> accounts, Colleague colleague, int page)
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();

        var page_size = 20;
        var total = accounts.Count();
        var counter = 0;
        accounts = accounts.Skip((page - 1) * page_size).Take(page_size).ToList();
        if (accounts.Count % 2 == 0)
        {
            for (var i = 0; i < accounts.Count; i += 2)
            {
                buttonLines.Add(new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"🔖 {accounts[i].UserName} {accounts[i].GetAccountStateEmoji()}{(accounts[i].Note.HasValue() ? $" / {accounts[i].Note}" : "")}",
                        $"{Constants.AccountConstants}-account*{accounts[counter].AccountCode}"),
                    InlineKeyboardButton.WithCallbackData(
                        $"🔖 {accounts[i + 1].UserName} {accounts[i + 1].GetAccountStateEmoji()}{(accounts[counter + 1].Note.HasValue() ? $" / {accounts[counter + 1].Note}" : "")}",
                        $"{Constants.AccountConstants}-account*{accounts[counter + 1].AccountCode}")
                });
                counter += 2;
            }
        }
        else
        {
            if (accounts.Count > 1)
                foreach (var account in accounts)
                    buttonLines.Add(new List<InlineKeyboardButton>()
                    {
                        InlineKeyboardButton.WithCallbackData(
                            $"🔖 {account.UserName} {account.GetAccountStateEmoji()}{(account.Note.HasValue() ? $" / {account.Note}" : "")}",
                            $"{Constants.AccountConstants}-account*{account.AccountCode}")
                    });
            else
                buttonLines.Add(new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"🔖 {accounts[^1].UserName} {accounts[^1].GetAccountStateEmoji()}{(accounts[^1].Note.HasValue() ? $" / {accounts[^1].Note}" : "")}",
                        $"{Constants.AccountConstants}-account*{accounts[^1].AccountCode}")
                });
        }

        var p = page <= total / 20;
        if (p)
        {
            if (page != 1)
                buttonLines.Add(new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData($"صفحه قبل 👈",
                        $"{Constants.AccountConstants}-page*{page - 1}"),
                    InlineKeyboardButton.WithCallbackData($"👉 صفحه بعد",
                        $"{Constants.AccountConstants}-page*{page + 1}")
                });
            else
                buttonLines.Add(new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData($"👉 صفحه بعد",
                        $"{Constants.AccountConstants}-page*{page + 1}")
                });
        }
        else
        {
            if (accounts.Count() != 0)
                buttonLines.Add(new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData($"صفحه قبل 👈",
                        $"{Constants.AccountConstants}-page*{page - 1}")
                });
        }

        return new InlineKeyboardMarkup(buttonLines);
    }

    public static IReplyMarkup SellerConfig(Account account)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData($"افزودن یادداشت 📝",
                    $"{Constants.AccountConstants}-note*{account.AccountCode}"),
                InlineKeyboardButton.WithCallbackData($"فروخته شد  ✅",
                    $"{Constants.AccountConstants}-sold*{account.AccountCode}")
            }
        });
    }

    public static IReplyMarkup ConfigManagement(PanelClientDto client, Server server, Account account)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData("حذف کانفیگ ✖️",
                    $"{Constants.AccountConstants}-cnf*rm*{account.AccountCode}*{server.Id}")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("ریست کانفیگ ♻️️",
                    $"{Constants.AccountConstants}-cnf*reset*{account.AccountCode}*{server.Id}")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("تمدید 📌️",
                    $"{Constants.AccountConstants}-cnf*extend*{account.AccountCode}*{server.Id}"),
                InlineKeyboardButton.WithCallbackData(
                    $"{(account.State.Equals(AccountState.Active) ? "مسدودی 🔴" : "رفع مسدودی 🟢")}",
                    $"{Constants.AccountConstants}-cnf*{(account.State.Equals(AccountState.Active) ? "block" : "unblock")}*{account.AccountCode}*{server.Id}")
            }
        });
    }

    public static IReplyMarkup ReporSellertAccount(Account account)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            // new()
            // {
            //     InlineKeyboardButton.WithCallbackData($"♻️ حذف کانفیگ",
            //         $"{Constants.AccountConstants}-removeconfg*{account.AccountCode}")
            // },
            // new()
            // {
            //     InlineKeyboardButton.WithCallbackData($"🔗️ تغییر پروتکل", $"updatetransmision*{account.AccountCode}")
            // },
            // new()
            // {
            //     InlineKeyboardButton.WithCallbackData($"♻  ریست لینک",
            //         $"{Constants.AccountConstants}-reseturl*{account.AccountCode}")
            // },
            // new()
            // {
            //     InlineKeyboardButton.WithCallbackData($"🌐️ جابجایی سرور", $"migrateconfig*{account.AccountCode}")
            // },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"⌛️ تمدید سرویس",
                    $"{Constants.AccountConstants}-extend*{account.AccountCode}"),
                InlineKeyboardButton.WithCallbackData($"افزودن یادداشت 📝",
                    $"{Constants.AccountConstants}-note*{account.AccountCode}")
            },
            // new()
            // {
            //     InlineKeyboardButton.WithCallbackData($"🐢 گزارش اختلال️",
            //         $"{Constants.AccountConstants}-reportlowspeed*{account.AccountCode}"),
            //     InlineKeyboardButton.WithCallbackData($"🔴 گزارش قطعی",
            //         $"{Constants.AccountConstants}-reportdisconect*{account.AccountCode}")
            // }
        });
    }
}