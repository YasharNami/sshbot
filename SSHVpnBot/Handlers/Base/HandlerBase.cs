using SSHVpnBot.Repositories.Uw;
using SSHVpnBot.Services.Logger;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SSHVpnBot.Handlers.Base;

public class HandlerBase : IHandlerBase
{
    public HandlerBase(IUnitOfWork uw, TelegramBotClient bot, ILoggerService logger)
    {
        _uw = uw;
        _bot = bot;
        _logger = logger;
    }

    public IUnitOfWork _uw { get; set; }
    public ILoggerService _logger { get; set; }
    public TelegramBotClient _bot { get; set; }

    public virtual Task Run(Update update)
    {
        throw new NotImplementedException();
    }

    public virtual Task Run()
    {
        throw new NotImplementedException();
    }
}