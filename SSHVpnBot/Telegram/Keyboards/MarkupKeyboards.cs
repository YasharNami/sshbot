using SSHVpnBot.Components.Subscribers;
using Telegram.Bot.Types.ReplyMarkups;

namespace SSHVpnBot.Telegram.Keyboards;

public class MarkupKeyboards
{
    public static ReplyKeyboardMarkup Main(Role role)
    {
        if (role == Role.Colleague)
            return new ReplyKeyboardMarkup(
                    new List<List<KeyboardButton>>
                    {
                        new() { "🔗 خرید اشتراک جدید" },
                        new() { "💰 شارژ کیف پول" },
                        new() { "🔎 استعلام اشتراک", "👤 حساب کاربری" },
                        new() { "📁 سفارشات من", "💬 پشتیبانی" },
                        // new() { "🤖 دریافت ربات نمایندگی" },
                    }
                )
                { ResizeKeyboard = true };
        else
            return new ReplyKeyboardMarkup(
                    new List<List<KeyboardButton>>
                    {
                        new() { "🔗 خرید اشتراک جدید" },
                        new() { "🎯 اشتراک های من", "🧪 دریافت اکانت تست" },
                        new() { "🗣 کسب درآمد","💰 شارژ کیف پول" },
                        new() { "💬 پشتیبانی", "👤 حساب کاربری" },
                    }
                )
                { ResizeKeyboard = true };
    }
    public static ReplyKeyboardMarkup ShareContact()
    {
        var x = new ReplyKeyboardMarkup(
            new List<List<KeyboardButton>>(){
                new  List<KeyboardButton>()  { "اشتراک گذاری شماره تماس 📞"},
                new  List<KeyboardButton>()  { "مرحله بعد 👉️" }});
        x.Keyboard.ToList()[0].ToList()[0].RequestContact = true;
        x.ResizeKeyboard = true;
        return x;
    }
    public static ReplyKeyboardMarkup Cancel()
    {
        return new ReplyKeyboardMarkup(new List<List<KeyboardButton>> { new() { "انصراف ✖️" } })
            { ResizeKeyboard = true };
    }
}