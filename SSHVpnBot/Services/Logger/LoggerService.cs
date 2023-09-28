using ConnectBashBot.Telegram.Handlers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Services.Logger;

public class LoggerService : ILoggerService
{
    
    public async Task LogMessage(ITelegramBotClient _bot, Update update, string step)
    {
        // Console.WriteLine($"from: {update.Message.From.FirstName + " " + update.Message.From.LastName}\n" +
        //                   $"message: {update.Message.Text}\n" +
        //                   $"step: {step}");
        await _bot.SendTextMessageAsync(MainHandler._loggroup,
            $"👤 from: <a href='tg://user?id={update.Message?.From?.Id}'>{update.Message?.From?.FirstName + " " + update.Message?.From?.LastName}</a>\n" +
            $"💬 message: {update.Message.Text}\n" +
            $"📍 step: <b>{step}</b>\n\n" +
            $"🔖 #U{update.Message.From.Id}",parseMode: ParseMode.Html);
    }

    public async Task LogCallback(ITelegramBotClient _bot, Update update)
    {
        // Console.WriteLine($"from: {update.CallbackQuery.From.FirstName + " " + update.CallbackQuery.From.LastName}\n" +
        //                   $"data: {update.CallbackQuery.Data}");
        await _bot.SendTextMessageAsync(MainHandler._loggroup,
            $"👤 from: <a  href='tg://user?id={update?.CallbackQuery?.From?.Id}'>{update.CallbackQuery.From.FirstName + " " + update.CallbackQuery.From.LastName}</a>\n" +
            $"🔗 data: <b>{update.CallbackQuery.Data}</b>\n\n" +
            $"🔖 #U{update.CallbackQuery.From.Id}",parseMode: ParseMode.Html);
    }

    public async Task LogException(ITelegramBotClient _bot, string message)
    {
        // Console.WriteLine($"exception recived:\n" +
        //                   $"{message}");
        await Program.logger_bot.SendTextMessageAsync(MainHandler._loggroup,
            $"exception recived:\n,{message}");
    }
}