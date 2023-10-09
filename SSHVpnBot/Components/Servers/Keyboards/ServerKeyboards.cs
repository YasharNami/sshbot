using ConnectBashBot.Commons;
using SSHVpnBot.Components.Accounts;
using SSHVpnBot.Components.Locations;
using SSHVpnBot.Components.ServiceCategories;
using SSHVpnBot.Repositories.Uw;
using Telegram.Bot.Types.ReplyMarkups;

namespace SSHVpnBot.Components.Servers.Keyboards;

public class ServerKeyboards
{
    public static IReplyMarkup ServerLocations(Server server, IEnumerable<Location> locations, int message_id)
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();

        foreach (var location in locations)
            buttonLines.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData(
                    $"{location.Title} ({location.Flat})",
                    $"{Constants.ServerConstants}-location*{server.Code}*{location.Code}*{message_id}")
            });
        return new InlineKeyboardMarkup(buttonLines);
    }

    public static async Task<IReplyMarkup> MigrateConfigServers(IUnitOfWork uw, List<Server> servers, Account account)
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();
        foreach (var server in servers)
        {
            Location location = null;
            if (server.LocationCode.HasValue())
                location = await uw.LocationRepository.GetLocationByCode(server.LocationCode);
            buttonLines.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData(
                    $"{(location is not null ? $"{location.Flat}" : "🟢")} {server.Domain.Split(".")[0]} ({server.Capacity}) {(server.Note.HasValue() ? $"| 📝 {server.Note}" : "")}",
                    $"{Constants.ServerConstants}-migratetoserver*{server.Code}*{account.AccountCode}")
            });
        }


        return new InlineKeyboardMarkup(buttonLines);
    }

    public static IReplyMarkup ColleagueServersManagement(List<Server> servers)
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();

        foreach (var server in servers)
            buttonLines.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData(
                    $"{(server.IsActive ? "🟢" : "🔴")} {server.Domain.Split(".")[0]} ({server.Capacity})",
                    $"{Constants.ServerConstants}-server*{server.Code}")
            });
        buttonLines.Add(new List<InlineKeyboardButton>()
        {
            InlineKeyboardButton.WithCallbackData($"همگامسازی سرورها ♻️‍", $"{Constants.ServerConstants}-colleagueserverssync")
        });
        return new InlineKeyboardMarkup(buttonLines);
    }

    public static async Task<IReplyMarkup> ServerManagement(IUnitOfWork uw, IEnumerable<Location> locations,
        List<Server> servers, int page,
        int total)
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();

        var page_size = 20;
        var main_servers = servers.Where(s => s.Type == ServerType.Main).ToList();

        var counter = 0;

        if (main_servers.Count % 2 == 0)
        {
            for (var i = 0; i < main_servers.Count; i += 2)
            {
                var first_server_reports =
                    (await uw.AccountReportRepository.GetAllOpenByServerCodeAsync(main_servers[counter].Code)).Count;
                var second_server_reports =
                    (await uw.AccountReportRepository.GetAllOpenByServerCodeAsync(main_servers[counter + 1].Code)).Count;

                buttonLines.Add(new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"{(main_servers[counter].IsActive ? main_servers[counter].LocationCode.HasValue() ? $"{locations.FirstOrDefault(s => s.Code.Equals(main_servers[counter].LocationCode)).Flat}" : "🟢" : "🔴")} {main_servers[counter].Domain} ({main_servers[counter].Capacity}) {(first_server_reports != 0 ? $"| {first_server_reports} ⚠️" : "")}",
                        $"{Constants.ServerConstants}-server*{main_servers[counter].Code}"),
                    InlineKeyboardButton.WithCallbackData(
                        $"{(main_servers[counter + 1].IsActive ? main_servers[counter + 1].LocationCode.HasValue() ? $"{locations.FirstOrDefault(s => s.Code.Equals(main_servers[counter + 1].LocationCode)).Flat}" : "🟢" : "🔴")} {main_servers[counter + 1].Domain} ({main_servers[counter + 1].Capacity}) {(second_server_reports != 0 ? $"| {second_server_reports} ⚠️" : "")}",
                        $"{Constants.ServerConstants}-server*{main_servers[counter + 1].Code}")
                });
                counter += 2;
            }
        }
        else
        {
            if (main_servers.Count > 1)
            {
                foreach (var main_server in main_servers)
                {
                    var server_reports =
                        (await uw.AccountReportRepository.GetAllOpenByServerCodeAsync(main_server.Code)).Count();

                    buttonLines.Add(new List<InlineKeyboardButton>()
                    {
                        InlineKeyboardButton.WithCallbackData(
                            $"{(main_server.IsActive ? main_server.LocationCode.HasValue() ? $"{locations.FirstOrDefault(s => s.Code.Equals(main_server.LocationCode)).Flat}" : "🟢" : "🔴")} " +
                            $"{main_server.Domain} ({main_server.Capacity}) " +
                            $"{(server_reports != 0 ? $"| {server_reports} ⚠️" : "")}",
                            $"{Constants.ServerConstants}-server*{main_server.Code}")
                    });
                }
            }
            else
            {
                var server_reports =
                    (await uw.AccountReportRepository.GetAllOpenByServerCodeAsync(main_servers[^1].Code)).Count();

                buttonLines.Add(new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"{(main_servers[^1].IsActive ? main_servers[^1].LocationCode.HasValue() ? $"{locations.FirstOrDefault(s => s.Code.Equals(main_servers[^1].LocationCode)).Flat}" : "🟢" : "🔴")} {main_servers[^1].Domain.Split(".")[0]} ({main_servers[^1].Capacity}) {(server_reports != 0 ? $"| {server_reports} ⚠️" : "")}",
                        $"{Constants.ServerConstants}-server*{main_servers[^1].Code}")
                });
            }
        }

        var p = page <= total / 20;
        if (p)
        {
            if (page != 1)
                buttonLines.Add(new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData($"صفحه قبل 👈", $"{Constants.ServerConstants}-page*{page - 1}"),
                    InlineKeyboardButton.WithCallbackData($"👉 صفحه بعد", $"{Constants.ServerConstants}-page*{page + 1}")
                });
            else
                buttonLines.Add(new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData($"👉 صفحه بعد", $"{Constants.ServerConstants}-page*{page + 1}")
                });
        }
        else
        {
            if (main_servers.Count != 0)
            {
                buttonLines.Add(new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData($"صفحه قبل 👈", $"{Constants.ServerConstants}-page*{page - 1}")
                });
            }
        }

        if (page == 1)
        {
            var check_servers = servers.Where(s => s.Type == ServerType.Check).ToList();
            if (check_servers.Count > 0)
                foreach (var server in check_servers)
                    buttonLines.Add(new List<InlineKeyboardButton>()
                    {
                        InlineKeyboardButton.WithCallbackData($"🌐 {server.Domain.Split(".")[0]}",
                            $"{Constants.ServerConstants}-server*{server.Code}")
                    });
        }

        // buttonLines.Add(new List<InlineKeyboardButton>()
        // {
        //     InlineKeyboardButton.WithCallbackData($"سرور همکاران 👨‍💻", $"{Constants.ServerConstants}-colleagueservers")
        // });
        buttonLines.Add(new List<InlineKeyboardButton>()
        {
            InlineKeyboardButton.WithCallbackData($"افزودن سرور جدید ➕", $"{Constants.ServerConstants}-newserver")
        });
        return new InlineKeyboardMarkup(buttonLines);
    }

    public static IReplyMarkup ServerManagement(List<Server> servers)
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();

        var mainServers = servers.Where(s => s.Type == ServerType.Main).OrderBy(s => s.Domain).ToList();
        for (var i = 0; i < mainServers.Count; i++)
            buttonLines.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData(
                    $"{(mainServers[i].IsActive ? "🟢" : "🔴")} {mainServers[i].Domain.Split(".")[0]} ({mainServers[i].Capacity})",
                    $"{Constants.ServerConstants}-server*{mainServers[i].Code}")
            });
        
        var checkServers = servers.Where(s => s.Type == ServerType.Check).ToList();
        if (checkServers.Count > 0)
            foreach (var server in checkServers)
                buttonLines.Add(new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"{(server.IsActive ? "🟢" : "🔴")} {server.Domain.Split(".")[0]} ({server.Capacity})",
                        $"{Constants.ServerConstants}-server*{server.Code}")
                });
        buttonLines.Add(new List<InlineKeyboardButton>()
        {
            InlineKeyboardButton.WithCallbackData($"افزودن سرور جدید ➕", $"{Constants.ServerConstants}-newserver")
        });
        return new InlineKeyboardMarkup(buttonLines);
    }

    public static IReplyMarkup ServerTypes(Server server)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData($"سرور تست 🧪️",
                    $"{Constants.ServerConstants}-type*{server.Code}*{nameof(ServerType.Check)}"),
                InlineKeyboardButton.WithCallbackData($"سرور اصلی ⚜️️",
                    $"{Constants.ServerConstants}-type*{server.Code}*{nameof(ServerType.Main)}")
            },
            // new()
            // {
            //     InlineKeyboardButton.WithCallbackData($"سرور همکار 👨‍💻️",
            //         $"{Constants.ServerConstants}-type*{server.Code}*{nameof(ServerType.Colleague)}")
            // }
        });
    }

    public static IReplyMarkup ServerCategories(Server server, List<ServiceCategory> categories, int message_id)
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();
        foreach (var item in categories)
            buttonLines.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData(
                    $"{(item.IsActive ? "🟢" : "🔴")} {item.Title}",
                    $"{Constants.ServerConstants}-category*{server.Code}*{item.Code}*{message_id}")
            });
        return new InlineKeyboardMarkup(buttonLines);
    }

    public static IReplyMarkup SingleServerManagement(Server server)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData("آدرس 🔗️️", $"{Constants.ServerConstants}-update*{server.Code}*url"),
                InlineKeyboardButton.WithCallbackData("ظرفیت کاربر 👥️",
                    $"{Constants.ServerConstants}-update*{server.Code}*capacity")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("نام کاربری 👤️",
                    $"{Constants.ServerConstants}-update*{server.Code}*username"),
                InlineKeyboardButton.WithCallbackData("رمز عبور 🔒️️️",
                    $"{Constants.ServerConstants}-update*{server.Code}*password")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("رمز ریموت 💻️",
                    $"{Constants.ServerConstants}-update*{server.Code}*sshpassword")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("ریبوت سرور ♻️",
                    $"{Constants.ServerConstants}-update*{server.Code}*reboot")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("دریافت گزارش اختلالات ⚠️️",
                    $"{Constants.ServerConstants}-update*{server.Code}*reports")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("ادرس دامنه 📍️️️️",
                    $"{Constants.ServerConstants}-update*{server.Code}*domain"),
                InlineKeyboardButton.WithCallbackData("نوع سرور 🔘️️️",
                    $"{Constants.ServerConstants}-update*{server.Code}*type")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("تست اتصال ♻️️️️",
                    $"{Constants.ServerConstants}-update*{server.Code}*check"),
                InlineKeyboardButton.WithCallbackData("حذف سرور ✖️️️️️",
                    $"{Constants.ServerConstants}-update*{server.Code}*delete")
            },
            // new()
            // {
            //     InlineKeyboardButton.WithCallbackData("همگامسازی سرور ♻️️️️",
            //         $"{Constants.ServerConstants}-update*{server.Code}*sync")
            // },
            new()
            {
                InlineKeyboardButton.WithCallbackData("دریافت بک آپ 🗂️",
                    $"{Constants.ServerConstants}-update*{server.Code}*backup")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("یادداشت 📝️",
                    $"{Constants.ServerConstants}-update*{server.Code}*note"),
                InlineKeyboardButton.WithCallbackData("لوکیشن 🌎️",
                    $"{Constants.ServerConstants}-update*{server.Code}*location")
            },
            // new()
            // {
            //     InlineKeyboardButton.WithCallbackData("دسته بندی سرور 🌀️",
            //         $"{Constants.ServerConstants}-update*{server.Code}*category")
            // },
            // new()
            // {
            //     InlineKeyboardButton.WithCallbackData("ساخت پورت های پیشفرض 🔧",
            //         $"{Constants.ServerConstants}-update*{server.Code}*initports")
            // },
            // new()
            // {
            //     InlineKeyboardButton.WithCallbackData("دریافت اکانت تست 🧪",
            //         $"{Constants.ServerConstants}-update*{server.Code}*account")
            // },
            new()
            {
                InlineKeyboardButton.WithCallbackData(
                    $"{(server.IsActive ? "غیرفعال سازی سرور 🔴️️️" : "فعال سازی سرور 🟢️️️️")}",
                    $"{Constants.ServerConstants}-update*{server.Code}*activation")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("افزودن سرور ✅",
                    $"{Constants.ServerConstants}-update*{server.Code}*done")
            }
        });
    }

    public static IReplyMarkup BackupOptions(Server server, string backupCode)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData($"جابجایی بک آپ 🧩",
                    $"{Constants.ServerConstants}-restorebackup*{server.Code}*{backupCode}")
            }
        });
    }

    public static IReplyMarkup RestoreBackupConfirmation(Server server, string backup_code)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData("بله 🟢️️",
                    $"{Constants.ServerConstants}-restoreconfirmation*{server.Code}*{backup_code}*approve"),
                InlineKeyboardButton.WithCallbackData("خیر 🔴️",
                    $"{Constants.ServerConstants}-restoreconfirmation*{server.Code}*{backup_code}*decline")
            }
        });
    }
}