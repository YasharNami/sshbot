using System.ComponentModel.DataAnnotations;
using ConnectBashBot.Commons;
using SSHVpnBot.Components.Colleagues;
using Telegram.Bot.Types.ReplyMarkups;

namespace SSHVpnBot.Components.Subscribers.Keyboards;

public class SubscriberKeyboards
{
    public static IReplyMarkup Members(int counts)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData($"{counts.ToString().En2Fa()} کاربر 👥",
                    $"none")
            }
        });
    }
    public static IReplyMarkup NotificationSettings()
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData($"✅️ فعال", $"{Constants.SubscriberConstatns}-notifications*turnon"),
                InlineKeyboardButton.WithCallbackData($"🚫 غیر فعال", $"{Constants.SubscriberConstatns}-notifications*turnoff")
            }
        });
    }
    public static IReplyMarkup SendToAllContactTypes()
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData($"👤 مشتریان️", $"{Constants.SubscriberConstatns}-contacts*subscribers"),
                InlineKeyboardButton.WithCallbackData($"🧑‍💻 فروشندگان", $"{Constants.SubscriberConstatns}-contacts*sellers")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"🧪 کاربران تست دریافت نکرده", $"{Constants.SubscriberConstatns}-contacts*notgettest")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"🔗 دارای اشتراک فعال", $"{Constants.SubscriberConstatns}-contacts*activeconfigs")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"📌 فاکتور های پرداخت نشده", $"{Constants.SubscriberConstatns}-contacts*factors")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"کذ تخفیف استفاده نکرده ها 🔖", $"{Constants.SubscriberConstatns}-contacts*notusetoffer")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"👥 تمام کاربران", $"{Constants.SubscriberConstatns}-contacts*all")
            }
        });
    }
    public static IReplyMarkup ColleagueHowToMeetUs()
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();
        foreach (var item in Enum.GetValues(typeof(HowMeetUs)))
        {
            DisplayAttribute displayAttribute = item.GetType()
                .GetField(item.ToString())
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .SingleOrDefault() as DisplayAttribute;

            string display = displayAttribute?.Name ?? item.ToString();
            
            buttonLines.Add(new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData(display, $"{Constants.SubscriberConstatns}-tocolleague*howtomeetus*{item}")
            });
        }
        return new InlineKeyboardMarkup(buttonLines);
    }
    public static IReplyMarkup ColleagueAverageOrder()
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();
        foreach (var item in Enum.GetValues(typeof(AverageOrder)))
        {
            DisplayAttribute displayAttribute = item.GetType()
                .GetField(item.ToString())
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .SingleOrDefault() as DisplayAttribute;

            string display = displayAttribute?.Name ?? item.ToString();
            
            buttonLines.Add(new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData(display, $"{Constants.SubscriberConstatns}-tocolleague*averageorder*{item}")
            });
        }
       
       
        return new InlineKeyboardMarkup(buttonLines);
    }
    
    public static IReplyMarkup ColleagueHowToSell()
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();
        foreach (var item in Enum.GetValues(typeof(HowToSell)))
        {
            DisplayAttribute displayAttribute = item.GetType()
                .GetField(item.ToString())
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .SingleOrDefault() as DisplayAttribute;

            string display = displayAttribute?.Name ?? item.ToString();
            
            buttonLines.Add(new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData(display, $"{Constants.SubscriberConstatns}-tocolleague*howtosell*{item}")
            });
        }
       
       
        return new InlineKeyboardMarkup(buttonLines);
    }
    
    public static IReplyMarkup BecomeColleague()
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData($"✅ بله، با اطمینان تائید میکنم",
                    $"{Constants.SubscriberConstatns}-settings*confirmcolleague")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"🚫 انصراف و بازگشت به منوی اصلی",
                    $"{Constants.SubscriberConstatns}-settings*back")
            }
        });
    }

    public static IReplyMarkup Profile(Subscriber user)
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();

        buttonLines.Add(new List<InlineKeyboardButton>
        {
            InlineKeyboardButton.WithCallbackData("💰️ شارژ کیف پول", $"{Constants.SubscriberConstatns}-chargewallet")
        });
        if (user.Role == Role.Subscriber)
            buttonLines.Add(new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData("🧑‍💻 تبدیل به حساب فروشنده",
                    $"{Constants.SubscriberConstatns}-settings*tocolleague")
            });
        buttonLines.Add(new List<InlineKeyboardButton>()
        {
            InlineKeyboardButton.WithCallbackData(
                $"{(user.Notification ? "✅ هشدار انقضای سرویس" : "🚫 هشدار انقضای سرویس")}",
                $"{Constants.SubscriberConstatns}-settings*notifications")
        });
        if (user.Role == Role.Colleague)
            buttonLines.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData("🔖 ویرایش ریمارک",
                    $"{Constants.SubscriberConstatns}-settings*updateremark")
            });

        // buttonLines.Add(new List<InlineKeyboardButton>()
        // {
        //     InlineKeyboardButton.WithCallbackData(
        //         $"🌀 جابجایی حساب کابری", $"{Constants.SubscriberConstatns}-settings*moveaccount")
        // });
        return new InlineKeyboardMarkup(buttonLines);
    }

    public static InlineKeyboardMarkup ChargeAmounts()
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData("۵۰،۰۰۰ تومان",
                    $"{Constants.SubscriberConstatns}-chargeamount*50000"),
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("۱۰۰،۰۰۰ تومان",
                    $"{Constants.SubscriberConstatns}-chargeamount*100000"),
                InlineKeyboardButton.WithCallbackData("۲۵۰،۰۰۰ تومان", $"{Constants.SubscriberConstatns}-chargeamount*250000")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("۵۰۰،۰۰۰ تومان",
                    $"{Constants.SubscriberConstatns}-chargeamount*500000"),
                InlineKeyboardButton.WithCallbackData("۱،۰۰۰،۰۰۰ تومان",
                    $"{Constants.SubscriberConstatns}-chargeamount*1000000")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("۲،۵۰۰،۰۰۰ تومان",
                    $"{Constants.SubscriberConstatns}-chargeamount*2500000"),
                InlineKeyboardButton.WithCallbackData("۵،۰۰۰،۰۰۰ تومان",
                    $"{Constants.SubscriberConstatns}-chargeamount*5000000")
            }
        });
    }

    public static IReplyMarkup SellerConfirmation(long chatId)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData("عدم تایید ✖️️",
                    $"{Constants.SubscriberConstatns}-checkseller*{chatId}*decline"),
                InlineKeyboardButton.WithCallbackData("تایید ✔️",
                    $"{Constants.SubscriberConstatns}-checkseller*{chatId}*approve")
            }
        });
    }

    public static IReplyMarkup SingleUserManagement(Subscriber subscriber, Colleague? colleague)
    {
        var next_level = "";
        if (colleague is not null)
            next_level = colleague.Level == ColleagueLevel.Base ? "سطح برنزی" :
                colleague.Level == ColleagueLevel.Bronze ? "سطح نقره ای" :
                colleague.Level == ColleagueLevel.Silver ? "سطح طلایی" :
                colleague.Level == ColleagueLevel.Gold ? "سطح حرفه ای" : "";
        return subscriber.Role == Role.Subscriber ? new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
            {
                new()
                {
                    InlineKeyboardButton.WithCallbackData($"ارسال پیام 💬",
                        $"{Constants.SubscriberConstatns}-tickettotuser*{subscriber.UserId}")
                },
                new()
                {
                    InlineKeyboardButton.WithCallbackData($"گزارش تراکنش ها 📉",
                        $"{Constants.SubscriberConstatns}-getreport*transactions*{subscriber.UserId}")
                },
                new()
                {
                    InlineKeyboardButton.WithCallbackData($"ارتفا به همکار 🎯",
                        $"{Constants.SubscriberConstatns}-changerole*colleague*{subscriber.UserId}")
                },
                new()
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"{(subscriber.IsActive ? "بن کردن کاربر 🔴" : "آنبن سازی کاربر 🟢")}",
                        $"{Constants.SubscriberConstatns}-useractivation*{(subscriber.IsActive ? "ban" : "unban")}*{subscriber.UserId}")
                }
            })
            : subscriber.Role == Role.Colleague ? new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
            {
                new()
                {
                    InlineKeyboardButton.WithCallbackData($"ارسال پیام 💬",
                        $"{Constants.SubscriberConstatns}-tickettotuser*{subscriber.UserId}")
                },
                new()
                {
                    InlineKeyboardButton.WithCallbackData($"گزارش تراکنش ها 📉",
                        $"{Constants.SubscriberConstatns}-getreport*transactions*{subscriber.UserId}")
                },
                new()
                {
                    InlineKeyboardButton.WithCallbackData($"تنزل به کاربر 👤",
                        $"{Constants.SubscriberConstatns}-changerole*subscriber*{subscriber.UserId}")
                },
                new()
                {
                    InlineKeyboardButton.WithCallbackData($"ارتقا به {next_level} 🔝",
                        $"{Constants.SubscriberConstatns}-upgradelevel*{colleague.UserId}")
                },
                new()
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"{(subscriber.IsActive ? "بن کردن کاربر 🔴" : "آنبن سازی کاربر 🟢")}",
                        $"{Constants.SubscriberConstatns}-useractivation*{(subscriber.IsActive ? "ban" : "unban")}*{subscriber.UserId}")
                }
            })
            : new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
            {
            });
    }
}