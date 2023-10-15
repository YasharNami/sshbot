using SSHVpnBot.Components.Orders;
using SSHVpnBot.Components.Services;
using SSHVpnBot.Repositories.Uw;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SSHVpnBot.Components.Servers;

public static class ServerUtillities
{
    public static async Task<bool> CheckServerCapacityForOrder(this ITelegramBotClient _bot, IUnitOfWork _uw,
        Order order,
        Service service, CallbackQuery callBackQuery)
    {
        var hasCapacity = await _uw.ServerRepository.Capacity(order.Count * service.UserLimit);
        if (!hasCapacity)
            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "عدم ظرفیت سرور 🔴", true);
        else
            await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "فیش با موفقیت تایید شد.✅", true);

        return hasCapacity;
    }
}