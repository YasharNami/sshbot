using Telegram.Bot;
using Telegram.Bot.Types;

namespace SSHVpnBot.Services.Logger;

public interface ILoggerService
{
    Task LogMessage(ITelegramBotClient _bot, Update update, string step);
    Task LogCallback(ITelegramBotClient _bot, Update update);

    Task LogException(ITelegramBotClient _bot, string message);
}