using SSHVpnBot.Components.Servers;
using SSHVpnBot.Services.Panel.Models;

namespace SSHVpnBot.Services.Panel;

public interface IPanelService
{
    Task<List<PanelClientDto>> GetAllUsersAsync(Server server);
    Task<bool> CreateNewClientAsync(Server server, CreateNewClientDto client);
    Task<bool> UpdateClientAsync(Server server, UpdateClientDto client);
    Task<bool> DeleteClientAsync(Server server, DeleteClientDto client);
    Task<bool> SuspendClientAsync(Server server, SuspendClientDto client);
    Task<bool> UnSuspendClientAsync(Server server, UnSuspendClientDto client);
    Task<bool> GetClientTrafficAsync(Server server, GetClientTrafficsDto client);
    Task<UpdateClientPasswordResponse?> UpdateClientPasswordAsync(Server server, UpdateClientPasswordDto client);
    Task<List<OnlineClient>> GetOnlineClientsAsync(Server server);
}