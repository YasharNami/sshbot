using Telegram.Bot.Types;

namespace SSHVpnBot.Components.Subscribers;

public static class SubscriberUtillities
{
    public static async Task<Subscriber> CreateSubscriberAsync(this User user)
    {
        return new Subscriber()
        {
            UserId = user.Id,
            Username = user.Username,
            Step = "none",
            isActive = true,
            Notification = true,
            JoinedOn = DateTime.Now,
            Role = Role.Subscriber,
            FullName = user.FirstName + " " + user.LastName
        };
    }
}