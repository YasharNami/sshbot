using ConnectBashBot.Commons;
using ConnectBashBot.Telegram.Handlers;
using SSHVpnBot.Components;
using SSHVpnBot.Components.Configurations;
using SSHVpnBot.Components.PaymentMethods;
using SSHVpnBot.Components.Servers;
using Telegram.Bot.Types.ReplyMarkups;

namespace SSHVpnBot.Telegram.Keyboards;

public class InlineKeyboards
{
    public static InlineKeyboardMarkup Joined()
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new() { InlineKeyboardButton.WithUrl("عضویت در کانال 🔗️", $"https://t.me/https://t.me/{MainHandler._channel}") },
            new() { InlineKeyboardButton.WithCallbackData("عضو شدم ✔️", "joined") }
        });
    }

    public static InlineKeyboardMarkup Marketings()
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData("🔖 فاکتور های پرداخت نشده امروز", "marketing*todaynotpaidorders")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("🔗 اشتراک های کمتر از یک هفته", "marketing*lessthanweekaccounts")
            }
        });
    }

    public static IReplyMarkup PaymentTypeCarts(IEnumerable<Configuration> carts)
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();

        foreach (var cart in carts)
            buttonLines.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData($"💳 {cart.Type.ToDisplay()}", $"cart*{cart.Id}")
            });

        return new InlineKeyboardMarkup(buttonLines);
    }

    public static IReplyMarkup SingleCartMangement(Configuration cart)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData("صاحب کارت 👤️", $"updatecart*owner*{cart.Id}")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("شماره کارت 💳", $"updatecart*number*{cart.Id}")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("ثبت کارت ✅️️️️️️️", $"updatecart*done*{cart.Id}")
            }
        });
    }


    public static IReplyMarkup SendGatewayPaymentRecept(string payment_url, string trackingCode)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new() { InlineKeyboardButton.WithUrl("پرداخت 🌐", $"{payment_url}") },
            new() { InlineKeyboardButton.WithCallbackData("ارسال تصویر فیش 📄", $"{Constants.OrderConstants}-sendrecept*{trackingCode}") }
        });
    }


    public static InlineKeyboardMarkup AdminPanel()
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData("مدیریت سرور ها 🌐", $"{Constants.ServerConstants}-management"),
                InlineKeyboardButton.WithCallbackData("لوکیشن ها 🌎️", "locations-management")
            },
            // new()
            // {
            //     InlineKeyboardButton.WithCallbackData("مدیریت دسته بندی ها 🌀", $"{Constants.CategoryConstants}-management")
            // },
            new()
            {
                InlineKeyboardButton.WithCallbackData("سرویس ها 🧩", $"{Constants.ServiceConstants}-management"),
                InlineKeyboardButton.WithCallbackData("کاربران 👥", $"{Constants.SubscriberConstatns}-management")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("مدیریت پرداخت 💳", "payment-management"),
                InlineKeyboardButton.WithCallbackData("کدهای تخفیف 🎉", $"{Constants.DiscountConstants}-discounts")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("ویرایش اطلاعات بانکی 💳", "payment-updatecart")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData("ارسال پیام همگانی 📩",$"{Constants.SubscriberConstatns}-sendtoall")
            },
            // new()
            // {
            //     InlineKeyboardButton.WithCallbackData("Marketing Strategies 🏓", "marketing-management")
            // }
        });
    }

    public static IReplyMarkup PaymentMethodsManaement(IEnumerable<PaymentMethod> methods)
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();
        foreach (var item in methods)
        {
            var status = item.IsActive ? "(فعال ✅)" : "(غیرفعال ☑️)";
            var emoji = item.IsActive ? "active" : "deactive)️";
            buttonLines.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData(
                    $"{item.Title} {status}", $"paymenttype*{item.Id}*{emoji}")
            });
        }

        return new InlineKeyboardMarkup(buttonLines);
    }
    
    
    public static IReplyMarkup AboutRemark()
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData($"بیشتر به من توضیح بده 💬️", $"aboutremark")
            }
        });
    }


    public static IReplyMarkup LearnCheck()
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithUrl($"راهنما 📝", $"https://t.me/connectbash/159"),
                InlineKeyboardButton.WithCallbackData($"ارتباط با پشتیبانی  🧑‍💻", $"help*contact")
            }
        });
    }

    public static IReplyMarkup Help()
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithUrl($"راهنمای اتصال و سایر آموزش ها", $"https://t.me/connectbash/309")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"نحوه ثبت گزارش کندی و اختـلال", $"help*lowspeed")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"نحوه تمدید اکانت خـریداری شده", $"help*extend")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"اکانت های مسدود شده", $"help*block")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"قـــــوانیـن و مـــقــررات ما", $"help*rules")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"ارتباط آنلاین با پشتیبانی", $"help*contact")
            }
        });
    }

    public static IReplyMarkup BackHelp()
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData($"بازگشت", $"backhelp")
            }
        });
    }

    public static IReplyMarkup BackHelpContact()
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithUrl($"ارتباط با پشتیبانی", $"https://t.me/connect_bash")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"بازگشت", $"backhelp")
            }
        });
    }

    public static IReplyMarkup Representations()
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData($"🥉 سیستم فروش برنزی", $"representation*bronze")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"🥈 سیستم فروش نقره ای", $"representation*silver")
            },
            new()
            {
                InlineKeyboardButton.WithCallbackData($"🏅 سیستم فروش طلایی", $"representation*golden")
            }
        });
    }

    public static IReplyMarkup CheckoutReferral()
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new() { InlineKeyboardButton.WithCallbackData("درخواست برداشت وجه 💰", $"checkoutreferral") }
        });
    }

    public static IReplyMarkup CheckoutAdminConfirmation(string code)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData("تایید 🟢", $"admincheckoutcnofirm*{code}*approve"),
                InlineKeyboardButton.WithCallbackData("عدم تایید 🔴", $"admincheckoutcnofirm*{code}*decline")
            }
        });
    }

    public static IReplyMarkup CheckoutConfirmation(string code)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData("تایید 🟢", $"checkoutconfirm*{code}*approve"),
                InlineKeyboardButton.WithCallbackData("عدم تایید 🔴", $"checkoutconfirm*{code}*decline")
            }
        });
    }

    public static IReplyMarkup TrackCheckout(string checkoutCode)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new() { InlineKeyboardButton.WithCallbackData("پیگیری برداشت وجه 🔍️️", $"trackcheckout*{checkoutCode}") }
        });
    }

    public static IReplyMarkup UnBlockOnIPScanner(Server server, string clientId, int port)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData($"رفع مسدودی 🟢",
                    $"unblock*approve*{clientId}*{server.Code}*{port}*ip")
            }
        });
    }
}