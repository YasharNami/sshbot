using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Telegram;

public static class TelegramUtillities
{
    public static async Task Choosed(this ITelegramBotClient _bot, CallbackQuery callBackQuery)
    {
        await _bot.AnswerCallbackQueryAsync(callBackQuery.Id, "انتخاب شد.");
    }
    public static async Task<bool> CheckJoin(this ITelegramBotClient _bot, long userId, ChatId channelId)
    {
        var join = await _bot.GetChatMemberAsync(channelId, userId);

        if (join.Status is not (ChatMemberStatus.Creator or ChatMemberStatus.Administrator or ChatMemberStatus.Member))
            return false;
        else
            return true;
    }
}