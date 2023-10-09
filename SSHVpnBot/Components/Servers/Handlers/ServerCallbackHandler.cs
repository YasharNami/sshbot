using ConnectBashBot.Commons;
using ConnectBashBot.Telegram.Handlers;
using SSHVpnBot.Components.Accounts;
using SSHVpnBot.Components.Base;
using SSHVpnBot.Components.Servers.Keyboards;
using SSHVpnBot.Components.Subscribers;
using SSHVpnBot.Repositories.Uw;
using SSHVpnBot.Services.Excel.Models;
using SSHVpnBot.Telegram;
using SSHVpnBot.Telegram.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Components.Servers.Handlers;

public class ServerCallbackHandler : QueryHandler
{
    public ServerCallbackHandler(ITelegramBotClient _bot, Update update,
        IUnitOfWork _uw,
        Subscriber subscriber, CancellationToken cancellationToken = default)
        :base(_bot,update,_uw,subscriber)
    {
        
    }
    public override async Task QueryHandlerAsync()
    {
        if (data.StartsWith("server*"))
        {
            var server = await _uw.ServerRepository.GetServerByCode(data.Replace("server*", ""));
            if (server is null)
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "سرور یافت نشد", true);

            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                "اطلاعات سرور مورد نظر جهت ویرایش برایتان ارسال شد.", true);

            // if (server.Username.HasValue() && server.Domain != "تنظیم نشده" &&
            //     server.Password != "تنظیم نشده" &&
            //     server.Url.HasValue())
            //     if (!server.Session.HasValue())
            //     {
            //         var login = await _uw.PanelService.Login(server);
            //         server.Session = StringExtension.Encrypt(login.session);
            //         _uw.ServerRepository.Update(server);
            //     }

            await _bot.SendTextMessageAsync(user.Id,
                $"<b>اطلاعات سرور <code>#{server.Code}</code></b>\n\n" +
                $"🔗 آدرس سرور :\n" +
                $"<code>{server.Url}</code>\n" +
                $"👤 نام کاربری : <b>{server.Username}</b>\n" +
                $"📍 آدرس دامنه : <b>{server.Domain}</b>\n" +
                $"👥 ظرفیت کاربر : <b>{server.Capacity} کاربر</b>\n",
                ParseMode.Html,
                replyMarkup: ServerKeyboards.SingleServerManagement(server));
        }
        else if (data.Equals("newserver"))
        {
            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                "اطلاعات سرور جدید جهت ویرایش برای شما ارسال شد.✔️", true);

            var serverCode = Server.GenerateNewCode();
            var server = new Server()
            {
                Code = serverCode,
                CreatedOn = DateTime.Now,
                IsActive = false,
                SSHPassword = MainHandler._sshPasswords,
                Username = MainHandler._v2rayUsernames,
                Password = MainHandler._v2rayPasswords,
                Capacity = MainHandler.server_capacities,
                Domain = "تنظیم نشده",
                Type = ServerType.Main,
                Url = "تنظیم نشده"
            };

            await _bot.AddNewServer(_uw, user.Id, server);
            _uw.ServerRepository.Add(server);
        }
        else if (data.StartsWith("restorebackup*"))
        {
            var server = await _uw.ServerRepository.GetServerByCode(data.Split("*")[1]);
            if (server is not null)
            {
                if (server.IsActive)
                    await _bot.SendTextMessageAsync(groupId,
                        $".\n" +
                        $"🧩 تاییدیه بازگردانی بک آپ\n\n" +
                        $"آیا از بازگردانی بک آپ فوق اطمینان دارید؟\n" +
                        $".",
                        ParseMode.Html,
                        replyToMessageId: callBackQuery.Message.MessageId,
                        replyMarkup: ServerKeyboards.RestoreBackupConfirmation(server,
                            data.Split("*")[2]));
                else
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "سرور مورد نظر غیرفعال می باشد.",
                        true);
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "سرور مورد نظر یافت نشد.", true);
            }
        }
        else if (data.StartsWith("update*"))
        {
            var server = await _uw.ServerRepository.GetServerByCode(data.Split("*")[1]);
            if (server is null)
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "سرور یافت نشد.", true);
                return;
            }

            var property = data.Split("*")[2];
            switch (property)
            {
                case "note":
                    await _bot.Choosed(callBackQuery);
                    await _bot.SendTextMessageAsync(user.Id, "📝 یادداشت سرور را ارسال کنید :",
                        replyMarkup: MarkupKeyboards.Cancel());
                    _uw.SubscriberRepository.ChangeStep(user.Id,
                        $"{Constants.ServerConstants}-update*{server.Code}*note*{callBackQuery.Message.MessageId}");
                    break;
                case "reports":
                    var reports = await _uw.AccountReportRepository.GetAllByServerCodeAsync(server.Code);
                    if (reports.Count != 0)
                    {
                        await _bot.Choosed(callBackQuery);

                        var result = new List<ServerReportsReportModel>();
                        foreach (var report in reports)
                        {
                            var u = await _uw.SubscriberRepository.GetByChatId(report.UserId);
                            if (u is not null)
                            {
                                var account =
                                    await _uw.AccountRepository.GetByAccountCode(report.AccountCode);
                                if (account is not null)
                                    result.Add(new ServerReportsReportModel()
                                    {
                                        AccountState = account.State.ToDisplay(),
                                        Operator = report.Operator.ToDisplay(),
                                        AnsweredBy = report.AnsweredBy == 0
                                            ? ""
                                            : report.AnsweredBy.ToString(),
                                        CreatedOn = report.CreatedOn.ConvertToPersianCalendar(),
                                        FullName = u.FullName,
                                        State = report.State.ToDisplay(),
                                        LastModifiedDate = report.LastModifiedDate != null
                                            ? report.LastModifiedDate.Value.ConvertToPersianCalendar()
                                            : "",
                                        Server = server.Domain,
                                        UserId = report.UserId,
                                        Code = report.Code,
                                        ClientId = account.ClientId,
                                        Url = account.Url,
                                        Description = report.Description != null
                                            ? report.Description
                                            : string.Empty,
                                        Type = report.Type.ToDisplay()
                                    });
                            }
                        }

                        await _uw.ExcelService.ServerReportsToCsv(server, result, _bot);
                    }
                    else
                    {
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                            "گزارشی بازی روی این سرور یافت نشد", true);
                    }

                    break;
                // case "category":
                //     await _bot.Choosed(callBackQuery);
                //     var categories = await _uw.ServiceCategoryRepository.GetAllCategoriesAsync();
                //     if (categories.Count is not 0)
                //         await _bot.SendTextMessageAsync(user.Id,
                //             "🌀️ دسته بندی سرور را انتخاب کنید :",
                //             replyMarkup: ServerKeyboards.ServerCategories(server,
                //                 categories,
                //                 callBackQuery.Message.MessageId));
                //     else
                //         await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "دسته بندی ای یافت نشد.",
                //             true);
                //     break;
                case "url":
                    await _bot.Choosed(callBackQuery);
                    await _bot.SendTextMessageAsync(user.Id, "🔗 آدرس سرور را ارسال کنید :",
                        replyMarkup: MarkupKeyboards.Cancel());
                    _uw.SubscriberRepository.ChangeStep(user.Id,
                        $"{Constants.ServerConstants}-update*{server.Code}*url*{callBackQuery.Message.MessageId}");
                    break;
                case "location":
                    var locations = _uw.LocationRepository.GetAll();
                    if (locations.Count() != 0)
                    {
                        await _bot.Choosed(callBackQuery);
                        await _bot.SendTextMessageAsync(user.Id, "🌎 لوکیشن سرور را انتخاب کنید :",
                            replyMarkup: ServerKeyboards.ServerLocations(server, locations,
                                callBackQuery.Message.MessageId));
                    }
                    else
                    {
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "لوکیشنی تعریف نشده است.");
                    }

                    break;
                case "type":
                    await _bot.Choosed(callBackQuery);
                    await _bot.SendTextMessageAsync(user.Id, "⚙️ نوع سرور را انتخاب کنید :",
                        replyMarkup: ServerKeyboards.ServerTypes(server));
                    break;
                case "sshport":
                    await _bot.Choosed(callBackQuery);
                    await _bot.SendTextMessageAsync(user.Id, "📍 پورت ریموت سرور را ارسال کنید :",
                        replyMarkup: MarkupKeyboards.Cancel());
                    _uw.SubscriberRepository.ChangeStep(user.Id,
                        $"{Constants.ServerConstants}-update*{server.Code}*sshport*{callBackQuery.Message.MessageId}");
                    break;
                case "username":
                    await _bot.Choosed(callBackQuery);
                    await _bot.SendTextMessageAsync(user.Id, "👤 نام کاربری سرور را ارسال کنید :",
                        replyMarkup: MarkupKeyboards.Cancel());
                    _uw.SubscriberRepository.ChangeStep(user.Id,
                        $"{Constants.ServerConstants}-update*{server.Code}*username*{callBackQuery.Message.MessageId}");
                    break;
                case "password":
                    await _bot.Choosed(callBackQuery);
                    await _bot.SendTextMessageAsync(user.Id, "🔒 رمز عبور سرور را ارسال کنید :",
                        replyMarkup: MarkupKeyboards.Cancel());
                    _uw.SubscriberRepository.ChangeStep(user.Id,
                        $"{Constants.ServerConstants}-update*{server.Code}*password*{callBackQuery.Message.MessageId}");
                    break;
                case "capacity":
                    await _bot.Choosed(callBackQuery);
                    await _bot.SendTextMessageAsync(user.Id,
                        "👥 ظرفیت کاربر سرور را به صورت عددی ارسال کنید :",
                        replyMarkup: MarkupKeyboards.Cancel());
                    _uw.SubscriberRepository.ChangeStep(user.Id,
                        $"{Constants.ServerConstants}-update*{server.Code}*capacity*{callBackQuery.Message.MessageId}");
                    break;
                case "domain":
                    await _bot.Choosed(callBackQuery);
                    await _bot.SendTextMessageAsync(user.Id, "📍 دامنه سرور را بدون https وارد نمایید :",
                        replyMarkup: MarkupKeyboards.Cancel());
                    _uw.SubscriberRepository.ChangeStep(user.Id,
                        $"{Constants.ServerConstants}-update*{server.Code}*domain*{callBackQuery.Message.MessageId}");
                    break;
                case "sshpassword":
                    await _bot.Choosed(callBackQuery);
                    await _bot.SendTextMessageAsync(user.Id, "💻 رمز عبور ریموت به سرور را وارد کنید :",
                        replyMarkup: MarkupKeyboards.Cancel());
                    _uw.SubscriberRepository.ChangeStep(user.Id,
                        $"{Constants.ServerConstants}-update*{server.Code}*sshpassword*{callBackQuery.Message.MessageId}");
                    break;
                case "check":
                    if (server.Url != "تنظیم نشده" && server.Username != "تنظیم نشده" &&
                        server.Password != "تنظیم نشده")
                    {
                        //var res = await _uw.PanelService.Login(server);
                        //
                        // server.Session = StringExtension.Encrypt(res.session);
                        // _uw.ServerRepository.Update(server);
                        // if (res.success)
                        //     await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                        //         "سرور با موفقیت متصل شد.✔️",
                        //         true);
                        // else
                        //     await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                        //         "خطایی در اتصال سرور پیش آمد.✔️",
                        //         true);
                    }
                    else
                    {
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                            $"پیش از تست سرور اطلاعات مورد نیاز را تکمیل نمایید.", true);
                    }

                    break;
                case "sync":
                    if (server.Url != "تنظیم نشده" && server.Username != "تنظیم نشده" &&
                        server.Password != "تنظیم نشده")
                    {
                        // var res = await _uw.PanelService.Login(server);
                        // server.Session = res.session.Encrypt();

                        Task.Run(() => { MainHandler.SyncServers(server); });
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                            "سرور با موفقیت همگامسازی شد.✔️",
                            true);

                        _uw.ServerRepository.Update(server);
                    }
                    else
                    {
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                            $"پیش از تست سرور اطلاعات مورد نیاز را تکمیل نمایید.", true);
                    }

                    break;
                case "reboot":
                    if (server.Url != "تنظیم نشده" && server.SSHPassword.HasValue())
                    {
                        await _bot.Choosed(callBackQuery);
                        //Task.Run(() => { _bot.RebootServerAsync(_uw, server, callBackQuery); });
                    }
                    else
                    {
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                            $"پیش از تست سرور اطلاعات مورد نیاز ریموت را تکمیل نمایید.", true);
                    }

                    break;
                case "activation":
                    server.IsActive = !server.IsActive;
                    _uw.ServerRepository.Update(server);
                    if (server.IsActive)
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                            "سرور با موفقیت فعال شد.", true);
                    else
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                            "سرور با موفقیت غیرفعال شد.", true);
                    await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
                    await _bot.SendTextMessageAsync(MainHandler._managementgroup,
                        $"<b>سرور به شرح زیر توسط {user.FirstName + " " + user.LastName} {(server.IsActive ? "فعال" : "غیرفعال")} شد.✔️</b>\n\n" +
                        $"🔖 شناسه سرور : <code>#{server.Code}</code>\n" +
                        $"🔗 آدرس سرور :\n" +
                        $" <code>{server.Url}</code>\n" +
                        $"🔘 نام کاربری : <code>{server.Username}</code>\n" +
                        $"👥 حداکقر ظرفیت : <b>{server.Capacity.ToString().En2Fa()} کاربر</b>\n",
                        ParseMode.Html);
                    break;
                case "delete":
                    server.IsRemoved = true;
                    _uw.ServerRepository.Update(server);
                    await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                        "سرور با موفقیت حذف شد.", true);

                    await _bot.SendTextMessageAsync(MainHandler._managementgroup,
                        $"<b>سرور به شرح زیر توسط {user.FirstName + " " + user.LastName} جذف شد.✔️</b>\n\n" +
                        $"🔖 شناسه سرور : <code>#{server.Code}</code>\n" +
                        $"🔗 آدرس سرور :\n" +
                        $" <code>{server.Url}</code>\n" +
                        $"🔘 نام کاربری : <code>{server.Username}</code>\n" +
                        $"👥 حداکقر ظرفیت : <b>{server.Capacity.ToString().En2Fa()} کاربر</b>\n",
                        ParseMode.Html);
                    break;
                case "done":
                    if (server.Url != "تنظیم نشده" && server.Domain != "تنظیم نشده" &&
                        server.Username != "تنظیم نشده" &&
                        server.Password != "تنظیم نشده")
                    {
                        // var res = await _uw.PanelService.Login(server);
                        // if (res.success)
                        // {
                        //     server.IsActive = true;
                        //     _uw.ServerRepository.Update(server);
                        //     await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
                        //     await _bot.SendTextMessageAsync(MainHandler._managementgroup,
                        //         $"<b>سرور جدید به شرح زیر توسط {user.FirstName + " " + user.LastName} ویرایش/اضافه شد.✔️</b>\n\n" +
                        //         $"🔖 شناسه سرور : <code>#{server.Code}</code>\n" +
                        //         $"🔗 آدرس سرور :\n" +
                        //         $" <code>{server.Url}</code>\n" +
                        //         $"🔘 نام کاربری : <code>{server.Username}</code>\n" +
                        //         $"👥 حداکقر ظرفیت : <b>{server.Capacity.ToString().En2Fa()} کاربر</b>\n",
                        //         ParseMode.Html);
                        // }
                        // else
                        // {
                        //     await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                        //         "نام کاربری یا رمز عبور سرور صحیحی نمی باشد.", true);
                        // }
                    }
                    else
                    {
                        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id,
                            "پیش از ویرایش سرور اطلاعات مورد نیاز را تکمیل نمایید.", true);
                    }

                    break;
                default:
                    break;
            }
        }
        else if (data.StartsWith("location*"))
        {
            var server = await _uw.ServerRepository.GetServerByCode(data.Split("*")[1]);
            if (server is not null)
            {
                var location = await _uw.LocationRepository.GetLocationByCode(data.Split("*")[2]);
                if (location is not null)
                {
                    await _bot.Choosed(callBackQuery);
                    await _bot.DeleteMessageAsync(user.Id, int.Parse(data.Split("*")[3]));
                    await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
                    server.LocationCode = location.Code;
                    _uw.ServerRepository.Update(server);
                    await _bot.AddNewServer(_uw, user.Id, server);
                }
                else
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "لوکیشن مورد نظر یافت نشد.",
                        true);
                }
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "سرور یافت نشد.", true);
            }
        }
        else if (data.StartsWith("category*"))
        {
            var server = await _uw.ServerRepository.GetServerByCode(data.Split("*")[1]);
            if (server is not null)
            {
                var category = await _uw.ServiceCategoryRepository.GetByServiceCategoryCode(data.Split("*")[2]);
                if (category is not null)
                {
                    await _bot.Choosed(callBackQuery);
                    //server.CategoryCode = category.Code;
                    _uw.ServerRepository.Update(server);
                    await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);
                    await _bot.DeleteMessageAsync(user.Id, int.Parse(data.Split("*")[3]));
                    await _bot.AddNewServer(_uw, user.Id, server);
                }
                else
                {
                    await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "دسته بندی مورد نظر یافت نشد.", true);
                }
            }
            else
            {
                await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "سرور مورد نظر یافت نشد.", true);
            }
        }
        else if (data.StartsWith("type*"))
        {
            await _bot.Choosed(callBackQuery);
            var server = await _uw.ServerRepository.GetServerByCode(data.Split("*")[1]);
            var type = data.Split("*")[2];
            server.Type = type == nameof(ServerType.Check) ? ServerType.Check : ServerType.Main;
            _uw.ServerRepository.Update(server);
            await _bot.DeleteMessageAsync(user.Id, callBackQuery.Message.MessageId);

            await _bot.AddNewServer(_uw, user.Id, server);
        }
        else if (data.Equals("management"))
        {
            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "مدیریت سرورها انتخاب شد.");
            var servers = await _uw.ServerRepository.GetAllByPaginationAsync(1, 20);

            // Task.Run(async () =>
            // {
            //     foreach (var server in servers.Where(s => s.IsActive).ToList())
            //     {
            //         var logined = await _uw.PanelService.Login(server);
            //         if (logined is null) await _bot.DeActiveServer(_uw, server);
            //     }
            // });
            servers = await _uw.ServerRepository.GetAllByPaginationAsync(1, 20);
            var actives = _uw.ServerRepository.ActiveCount();
            var deactives = _uw.ServerRepository.DectiveCount();
            var locations = _uw.LocationRepository.GetAll();
            await _bot.SendTextMessageAsync(groupId,
                $".\n" +
                $"🗂 صفحه <b>{1.En2Fa()}</b> از <b>{Math.Ceiling((decimal)(actives + deactives) / 20).En2Fa()}</b>\n\n" +
                $"🟢 <b>{actives.En2Fa()}</b> سرور فعال\n" +
                $"🔴 <b>{deactives.En2Fa()}</b> سرور غیرفعال\n\n" +
                $"قصد مدیریت کدام سرور را دارید؟\n" +
                $".",
                ParseMode.Html,
                replyMarkup: await ServerKeyboards.ServerManagement(_uw, locations, servers, 1,
                    actives + deactives));
        }
        else if (data.StartsWith("page*"))
        {
            await _bot.Choosed(callBackQuery);
            var page = int.Parse(data.Replace("page*", ""));
            var servers = await _uw.ServerRepository.GetAllByPaginationAsync(page, 20);

            // Task.Run(async () =>
            // {
            //     foreach (var server in servers.Where(s => s.IsActive).ToList())
            //     {
            //         var logined = await _uw.PanelService.Login(server);
            //         if (logined is null) await _bot.DeActiveServer(_uw, server);
            //     }
            // });
            servers = await _uw.ServerRepository.GetAllByPaginationAsync(page, 20);
            var actives = _uw.ServerRepository.ActiveCount();
            var deactives = _uw.ServerRepository.DectiveCount();
            await _bot.DeleteMessageAsync(groupId, callBackQuery.Message.MessageId);
            var locations = _uw.LocationRepository.GetAll();

            await _bot.SendTextMessageAsync(groupId,
                $".\n" +
                $"🗂 صفحه <b>{page.En2Fa()}</b> از <b>{Math.Ceiling((decimal)(actives + deactives) / 20).En2Fa()}</b>\n\n" +
                $"🟢 <b>{actives.En2Fa()}</b> سرور فعال\n" +
                $"🔴 <b>{deactives.En2Fa()}</b> سرور غیرفعال\n\n" +
                $"قصد مدیریت کدام سرور را دارید؟\n" +
                $".",
                ParseMode.Html,
                replyMarkup: await ServerKeyboards.ServerManagement(_uw, locations, servers, page,
                    actives + deactives));
        }
    }
}