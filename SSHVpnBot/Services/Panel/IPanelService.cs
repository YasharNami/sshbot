using SSHVpnBot.Components.Servers;
using SSHVpnBot.Services.Panel.Apis;
using SSHVpnBot.Services.Panel.Models;

namespace SSHVpnBot.Services.Panel;

public interface IPanelService
{
    Task<List<PanelClientDto>?> GetAllUsersAsync(Server server);
    Task<bool?> CreateNewClientAsync(Server server, CreateNewClientDto client);
    Task<ApiResult?> UpdateClientAsync(Server server, UpdateClientDto client);
    Task<ApiResult?> DeleteClientAsync(Server server, DeleteClientDto client);
    Task<ApiResult?> SuspendClientAsync(Server server, SuspendClientDto client);
    Task<ApiResult?> UnSuspendClientAsync(Server server, UnSuspendClientDto client);
   Task<ApiResult?> ExtendClientAsync(Server server, ExtendClientDto client);
    //Task<bool> GetClientTrafficAsync(Server server, GetClientTrafficsDto client);
    Task<List<OnlineClient>?> GetOnlineClientsAsync(Server server);

}