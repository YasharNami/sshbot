using SSHVpnBot.Components.Subscribers;
using SSHVpnBot.Repositories.Uw;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SSHVpnBot.Components.Base;

public abstract class MessageHandler
{
    
    public Message message;
    public User user;
    public ITelegramBotClient _bot;
    public Subscriber subscriber;
    public IUnitOfWork _uw;
    public string step;

    public MessageHandler(ITelegramBotClient _bot, Update update, IUnitOfWork _uw,
        Subscriber subscriber, CancellationToken cancellationToken = default)
    {
        this.subscriber = subscriber;
        this._uw = _uw;
        this._bot = _bot;
        message = update.Message;
        user = update.Message.From!;
        step = subscriber.Step.Equals("none") ? "none" : subscriber.Step.Split("-")[1];
    }
    public virtual async Task MessageHandlerAsync()
    {
    }
}