using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Subscribers.Repository;


public interface ISubscriberRepository : IBaseRepository<Subscriber>
{
    Task<Role> GetRoleAsync(long userid);
    Task<List<Subscriber>> GetReferralledAsync();
    Task<string?> CheckStep(long chatId);
    void ChangeStep(long chatId, string step);
    Task<Subscriber?> GetByUserName(string username);
    Task<Subscriber?> GetByChatId(long chatId);
    int GetCount();
    bool IsAdmin(long chatId);
    Task<List<Subscriber>> GetAdmins();
    Task<List<Subscriber>> GetAllChatIds();
    Task<int?> GetAllByReferral(string referral);
    Task<int> GetSellers();
    Task<int> Today();
    Task<int?> Yesterday();
}