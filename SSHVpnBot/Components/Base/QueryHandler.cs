using SSHVpnBot.Components.Subscribers;
using SSHVpnBot.Repositories.Uw;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SSHVpnBot.Components.Base;

public abstract class QueryHandler
{
    public Message message;
    public User user;
    public CallbackQuery callBackQuery;
    public long groupId;
    public string data;
    public ITelegramBotClient _bot;
    public Subscriber subscriber;
    public IUnitOfWork _uw;
    public QueryHandler(ITelegramBotClient _bot, Update update, IUnitOfWork _uw,
        Subscriber subscriber, CancellationToken cancellationToken = default)
    {
        this.subscriber = subscriber;
        this._uw = _uw;
        this._bot = _bot;
        callBackQuery = update.CallbackQuery!;
        data = update.CallbackQuery.Data.Split("-")[1];
        groupId = callBackQuery.Message.Chat.Id;
        message = update.Message;
        user = update.CallbackQuery.From!;
    }

    public virtual async Task QueryHandlerAsync()
    {
     
    }
}