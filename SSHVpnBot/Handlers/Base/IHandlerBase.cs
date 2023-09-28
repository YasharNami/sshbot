using Telegram.Bot.Types;

namespace SSHVpnBot.Handlers.Base;

public interface IHandlerBase
{
    Task Run(Update update);
    Task Run();
}