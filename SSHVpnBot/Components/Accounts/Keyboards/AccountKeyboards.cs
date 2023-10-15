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
                    $"🔗 {account.Email}",
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
                InlineKeyboardButton.WithCallbackData("🔍 جستجو بر اساس شناسه", $"{Constants.AccountConstants}-searchbyuid")
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
                    $"🔗 {account.Url.Split("#")[1]} / 📝 {account.Note}",
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
                InlineKeyboardButton.WithCallbackData($"⌛️ تمدید سرویس", $"{Constants.AccountConstants}-extend*{accountCode}")
            }
        });
    }

    public static IReplyMarkup RemoveConfigConfirmation(Server server, string clientId, int port)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData($"بله اطمینان دارم 👍",
                    $"{Constants.AccountConstants}-rmcnf*approve*{clientId}*{server.Id}*{port}")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"خیر مطمین نیستم 👍",
                    $"{Constants.AccountConstants}-rmcnf*decline*{clientId}*{server.Id}*{port}")
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
                },
                new()
                {
                    InlineKeyboardButton.WithCallbackData($"🐢 گزارش کندی و اختلال️",
                        $"{Constants.AccountReportConstants}-reportlowspeed*{account.AccountCode}")
                },
                new()
                {
                    InlineKeyboardButton.WithCallbackData($"🔴 گزارش قطعی",
                        $"{Constants.AccountReportConstants}-reportdisconect*{account.AccountCode}")
                }
            });
        else
            return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
            {
                new()
                {
                    InlineKeyboardButton.WithCallbackData($"♻️ دریافت مجدد کانفیگ️️",
                        $"{Constants.AccountConstants}-reget*{account.AccountCode}")
                },
                new()
                {
                    InlineKeyboardButton.WithCallbackData($"⌛️ تمدید سرویس",
                        $"{Constants.AccountConstants}-extend*{account.AccountCode}")
                },
                new()
                {
                    InlineKeyboardButton.WithCallbackData($"🐢 گزارش کندی و اختلال️",
                        $"{Constants.AccountReportConstants}-reportlowspeed*{account.AccountCode}")
                },
                new()
                {
                    InlineKeyboardButton.WithCallbackData($"🔴 گزارش قطعی",
                        $"{Constants.AccountReportConstants}-reportdisconect*{account.AccountCode}")
                }
            });
    }

    public static IReplyMarkup MineAccounts(List<Account> accounts, Colleague colleague, int page)
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();

        var page_size = 20;
        var total = accounts.Count();
        var counter = 0;
        var remarked = colleague.Tag.HasValue();
        accounts = accounts.Skip((page - 1) * page_size).Take(page_size).ToList();
        if (accounts.Count % 2 == 0)
        {
            for (var i = 0; i < accounts.Count; i += 2)
            {
                buttonLines.Add(new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"🔖 {accounts[counter].Url.Split("#")[1].Replace(remarked ? colleague.Tag : "ConnectBash", "")} {accounts[counter].GetAccountStateEmoji()}",
                        $"{Constants.AccountConstants}-account*{accounts[counter].AccountCode}"),
                    InlineKeyboardButton.WithCallbackData(
                        $"🔖 {accounts[counter + 1].Url.Split("#")[1].Replace(remarked ? colleague.Tag : "ConnectBash", "")} {accounts[counter + 1].GetAccountStateEmoji()}",
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
                            $"🔖 {account.Url.Split("#")[1].Replace(remarked ? colleague.Tag : "ConnectBash", "")} {account.GetAccountStateEmoji()}",
                            $"{Constants.AccountConstants}-account*{account.AccountCode}")
                    });
            else
                buttonLines.Add(new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"🔖 {accounts[^1].Url.Split("#")[1].Replace(remarked ? colleague.Tag : "ConnectBash", "")} {accounts[^1].GetAccountStateEmoji()}",
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
                InlineKeyboardButton.WithUrl("ورود به پنل 📊️",server.Url)
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("حذف کانفیگ ✖️",
                    $"{Constants.AccountConstants}-cnf*rm*{account.AccountCode}*{server.Id}")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("ریست کانفیگ ♻️️",
                    $"{Constants.AccountConstants}-cnf*reset*{account.AccountCode}*{server.Id}"),
                InlineKeyboardButton.WithCallbackData("ریست لینک ♻️️",
                    $"{Constants.AccountConstants}-cnf*reseturl*{account.AccountCode}*{server.Id}")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("تمدید 📌️",
                    $"{Constants.AccountConstants}-cnf*extend*{account.AccountCode}*{server.Id}"),
                InlineKeyboardButton.WithCallbackData($"{(account.State.Equals(AccountState.Active) ? "مسدودی 🔴" : "رفع مسدودی 🟢")}",
                    $"{Constants.AccountConstants}-cnf*{(account.State.Equals(AccountState.Active) ? "block" : "unblock")}*{account.AccountCode}*{server.Id}")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"🌐️ جابجایی سرور",
                    $"{Constants.AccountConstants}-migrateconfig*{account.AccountCode}")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"♻️ ارسال مجدد کانفیگ",
                    $"{Constants.AccountConstants}-cnf*resend*{account.AccountCode}*{server.Id}")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"🕒 {account.EndsOn.ConvertToPersianCalendar()}",
                    $"{Constants.AccountConstants}-cnf*none*{client.Username}*{server.Id}"),
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("کاهش روز ➖",
                    $"{Constants.AccountConstants}-cnf*minusday*{account.AccountCode}*{server.Id}"),
                InlineKeyboardButton.WithCallbackData("افزایش روز ➕",
                    $"{Constants.AccountConstants}-cnf*plusday*{account.AccountCode}*{server.Id}")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("➖",
                    $"{Constants.AccountConstants}-cnf*minus*{account.AccountCode}*{server.Id}"),
                InlineKeyboardButton.WithCallbackData($"📱 {client.Multiuser.En2Fa()} کاربر",
                    $"{Constants.AccountConstants}-cnf*none*{client.Username}*{server.Id}"),
                InlineKeyboardButton.WithCallbackData("➕",
                    $"{Constants.AccountConstants}-cnf*plus*{account.AccountCode}*{server.Id}")
            }
        });
    }

    public static IReplyMarkup ReporSellertAccount(Account account)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData($"♻️ حذف کانفیگ",
                    $"{Constants.AccountConstants}-removeconfg*{account.AccountCode}")
            },
            // new()
            // {
            //     InlineKeyboardButton.WithCallbackData($"🔗️ تغییر پروتکل", $"updatetransmision*{account.AccountCode}")
            // },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"♻️ دریافت مجدد",
                    $"{Constants.AccountConstants}-reget*{account.AccountCode}"),
                InlineKeyboardButton.WithCallbackData($"♻  ریست لینک",
                    $"{Constants.AccountConstants}-reseturl*{account.AccountCode}")
            },
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
            new()
            {
                InlineKeyboardButton.WithCallbackData($"🐢 گزارش اختلال️",
                    $"{Constants.AccountConstants}-reportlowspeed*{account.AccountCode}"),
                InlineKeyboardButton.WithCallbackData($"🔴 گزارش قطعی",
                    $"{Constants.AccountConstants}-reportdisconect*{account.AccountCode}")
            }
        });
    }
}