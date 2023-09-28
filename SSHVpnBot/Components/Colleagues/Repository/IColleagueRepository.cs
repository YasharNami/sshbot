using SSHVpnBot.Repositories;

namespace SSHVpnBot.Components.Colleagues.Repository;

public interface IColleagueRepository : IBaseRepository<Colleague>
{
    Task<Colleague?> GetByChatId(long chatId);
    Task<bool> RemarkIsExist(string remark);
    Task<int> Today();
    Task<int?> Yesterday();
}