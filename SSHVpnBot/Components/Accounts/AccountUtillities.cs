using ConnectBashBot.Commons;
using ConnectBashBot.Telegram.Handlers;
using SSHVpnBot.Components.Orders;
using SSHVpnBot.Components.Servers;
using SSHVpnBot.Components.Services;
using SSHVpnBot.Components.Subscribers;
using SSHVpnBot.Repositories.Uw;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Components.Accounts;

public static class AccountUtillities
{
    public static string GetAccountStateEmoji(this Account account)
    {
        switch (account.State)
        {
            case AccountState.Active:
                return "🟢";
            case AccountState.DeActive:
                return "🔴";
            case AccountState.Expired_Traffic:
                return "🔋";
            case AccountState.Expired:
                return "⌛️";
            case AccountState.Blocked:
                return "❌";
            case AccountState.Blocked_Ip:
                return "📍";
            default:
                return "";
        }
    }
}