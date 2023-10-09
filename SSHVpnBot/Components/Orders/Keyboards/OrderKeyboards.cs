using ConnectBashBot.Commons;
using SSHVpnBot.Components.PaymentMethods;
using SSHVpnBot.Components.ServiceCategories;
using SSHVpnBot.Components.Services;
using SSHVpnBot.Components.Subscribers;
using SSHVpnBot.Repositories.Uw;
using Telegram.Bot.Types.ReplyMarkups;

namespace SSHVpnBot.Components.Orders.Keyboards;

public class OrderKeyboards
{
    public static IReplyMarkup SendPaymentRecept(string trackingCode)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new() { InlineKeyboardButton.WithCallbackData("ارسال تصویر فیش 📄", $"{Constants.OrderConstants}-sendrecept*{trackingCode}") }
        });
    }

    public static IReplyMarkup SingleOrderManagement(Order order)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData("ارسال مجدد کانفیگ ♻️",
                    $"{Constants.OrderConstants}-update*{order.TrackingCode}*resend")
            }
        });
    }

    public static IReplyMarkup CreateOnColleagueServerConfirmation(Service service, Order order)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData($"تایید 🟢",
                    $"{Constants.OrderConstants}-crateonservercolleague*approve*{service.Code}*{order.TrackingCode}"),
                InlineKeyboardButton.WithCallbackData($"عدم تایید 🔴",
                    $"{Constants.OrderConstants}-crateonservercolleague*decline")
            }
        });
    }

    // public static async Task<IReplyMarkup> Categories(IUnitOfWork uw, Subscriber subscriber,
    //     List<ServiceCategory> categories, List<Service> services)
    // {
    //     var buttonLines = new List<List<InlineKeyboardButton>>();
    //
    //     if (subscriber.Role.Equals(Role.Colleague))
    //         foreach (var category in categories)
    //         {
    //             var service = services.Where(s => s.CategoryCode.Equals(category.Code)).OrderBy(s => s.Price)
    //                 .FirstOrDefault();
    //             if (service is not null)
    //             {
    //                 var baseprice = await uw.OfferRulesRepository.GetByServiceCode(service.Code);
    //                 if (baseprice is not null)
    //                     buttonLines.Add(new List<InlineKeyboardButton>()
    //                     {
    //                         InlineKeyboardButton.WithCallbackData($"🔗 {category.Title}" +
    //                                                               $" " +
    //                                                               $"{(service is not null ? $"شروع از {baseprice.BasePrice.ToIranCurrency().En2Fa()} تومان" : "")}",
    //                             $"{Constants.OrderConstants}-category*{category.Code}")
    //                     });
    //             }
    //         }
    //     else
    //         foreach (var category in categories)
    //         {
    //             var service = services.Where(s => s.CategoryCode.Equals(category.Code)).OrderBy(s => s.Price)
    //                 .FirstOrDefault();
    //             buttonLines.Add(new List<InlineKeyboardButton>()
    //             {
    //                 InlineKeyboardButton.WithCallbackData($"🔗 {category.Title}" +
    //                                                       $" " +
    //                                                       $"{(service is not null ? $"شروع از {service.Price.ToIranCurrency().En2Fa()} تومان" : "")}",
    //                     $"{Constants.OrderConstants}-category*{category.Code}")
    //             });
    //         }
    //
    //     return new InlineKeyboardMarkup(buttonLines);
    // }

    public static async Task<InlineKeyboardMarkup> Services(IUnitOfWork _uw, Subscriber subscriber,
        IEnumerable<Service> services)
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();
        if (subscriber.Role.Equals(Role.Colleague))
            foreach (var item in services.OrderBy(s => s.Price).ToList())
            {
                if (item.SellerPrice is not 0)
                    buttonLines.Add(new List<InlineKeyboardButton>()
                    {
                        InlineKeyboardButton.WithCallbackData(
                            $"{(item.Traffic != 0 ? "🔗" : "⚜️")} {item.GetFullTitle()} {item.SellerPrice.ToIranCurrency().En2Fa()} تومان",
                            $"{Constants.OrderConstants}-factor*{item.Id}")
                    });
            }
        else
            foreach (var item in services.OrderBy(s => s.Price).ToList())
                buttonLines.Add(new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"{(item.Traffic != 0 ? "🔗" : "⚜️")} {item.GetFullTitle()} {item.Price.ToIranCurrency().En2Fa()} تومان",
                        $"{Constants.OrderConstants}-factor*{item.Id}")
                });
        
        // buttonLines.Add(new List<InlineKeyboardButton>()
        // {
        //     InlineKeyboardButton.WithCallbackData($"👈 بازگشت به سرویس ها", $"{Constants.OrderConstants}-servicecategories")
        // });
        return new InlineKeyboardMarkup(buttonLines);
    }

    public static IReplyMarkup MineOrders(List<Order> orders)
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();
        foreach (var order in orders)
            buttonLines.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData($"🔖 #{order.TrackingCode} ({order.Count.En2Fa()} کانفیگ)",
                    $"{Constants.OrderConstants}-review*{order.TrackingCode}")
            });
        return new InlineKeyboardMarkup(buttonLines);
    }
    
    public static InlineKeyboardMarkup PaymentMethods(IEnumerable<PaymentMethod> methods, Service service,
        string trackingCode)
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();
        buttonLines.Add(new List<InlineKeyboardButton>()
        {
            InlineKeyboardButton.WithCallbackData($"کد تخفیف دارم 🔖", $"{Constants.OrderConstants}-discount*{trackingCode}")
        });
        foreach (var item in methods)
            buttonLines.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData($"{item.Title}",
                    $"{Constants.OrderConstants}-payment*{item.Id}*{service.Id}*{trackingCode}")
            });
        return new InlineKeyboardMarkup(buttonLines);
    }

    public static IReplyMarkup TrackPayment(string paymentCode)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData("پیگیری پرداخت 🔍️️",
                    $"{Constants.OrderConstants}-trackpayment*{paymentCode}")
            }
        });
    }

    public static InlineKeyboardMarkup PaymentMethodsAfterDiscount(IEnumerable<PaymentMethod> methods, Service service,
        string trackingCode)
    {
        var buttonLines = new List<List<InlineKeyboardButton>>();
        foreach (var item in methods)
            buttonLines.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData(
                    $"{item.Title}", $"{Constants.OrderConstants}-payment*{item.Id}*{service.Id}*{trackingCode}")
            });

        return new InlineKeyboardMarkup(buttonLines);
    }

    public static IReplyMarkup ReceptConfirmagtion(string code)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData("عدم تایید ✖️️",
                    $"{Constants.OrderConstants}-checkrecept*{code}*decline"),
                InlineKeyboardButton.WithCallbackData("تایید ✔️",
                    $"{Constants.OrderConstants}-checkrecept*{code}*approve")
            }
        });
    }

    public static IReplyMarkup TrackOrder(string trackingCode)
    {
        return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>
        {
            new()
            {
                InlineKeyboardButton.WithCallbackData("پیگیری سفارش 🔍️️",
                    $"{Constants.OrderConstants}-trackorder*{trackingCode}")
            }
        });
    }
}