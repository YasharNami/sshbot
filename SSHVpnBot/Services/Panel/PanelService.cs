using System.Net.Http.Headers;
using System.Net.Http.Json;
using ConnectBashBot.Commons;
using Newtonsoft.Json;
using SSHVpnBot.Components.Servers;
using SSHVpnBot.Services.Panel.Apis;
using SSHVpnBot.Services.Panel.Models;

namespace SSHVpnBot.Services.Panel;

public class PanelService : IPanelService
{
    protected readonly string token = "lHsurBaiZDyHMSKt";
    
    private static class Methods
    {
        public const string getAllClients = "alluser";
        public const string createNewClient = "adduser";
        public const string updateClient = "edituser";
        public const string deleteClient = "deleteuser";
        public const string suspendClient = "suspenduser";
        public const string unSuspendClient = "unsuspenduser";
        public const string getClientTraffics = "getusertraffic";
        public const string updateClientPassword = "changepassword";
        public const string getOnlineClients = "online";

    }
    public async Task<List<PanelClientDto>> GetAllUsersAsync(Server server)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync($"{server.Url}/apiV1/api.php?token={token}", 
                        new Request(Methods.getAllClients));
                var value = await httpResponseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ApiResult<List<PanelClientDto>>>(value);
                if (response.response_code.Equals(200))
                    return response.data;
                else return new List<PanelClientDto>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new List<PanelClientDto>();
            }
        }
    }
    public async Task<bool> CreateNewClientAsync(Server server,CreateNewClientDto client)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync($"{server.Url}/apiV1/api.php?token={token}", 
                        client);
                var value = await httpResponseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ApiResult<bool>>(value);
                if (response.response_code.Equals(200))
                {
                    return response.data;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
    public async Task<bool> UpdateClientAsync(Server server,UpdateClientDto client)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync($"{server.Url}/apiV1/api.php?token={token}", 
                        client);
                var value = await httpResponseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ApiResult<bool>>(value);
                if (response.response_code.Equals(200))
                    return response.data;
                else return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
    public async Task<bool> DeleteClientAsync(Server server,DeleteClientDto client)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync($"{server.Url}/apiV1/api.php?token={token}", 
                        client);
                var value = await httpResponseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ApiResult<bool>>(value);
                if (response.response_code.Equals(200))
                    return response.data;
                else return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
    public async Task<bool> SuspendClientAsync(Server server,SuspendClientDto client)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync($"{server.Url}/apiV1/api.php?token={token}", 
                        client);
                var value = await httpResponseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ApiResult<bool>>(value);
                if (response.response_code.Equals(200))
                    return response.data;
                else return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
    public async Task<bool> UnSuspendClientAsync(Server server,UnSuspendClientDto client)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync($"{server.Url}/apiV1/api.php?token={token}", 
                        client);
                var value = await httpResponseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ApiResult<bool>>(value);
                if (response.response_code.Equals(200))
                    return response.data;
                else return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
    public async Task<bool> GetClientTrafficAsync(Server server,GetClientTrafficsDto client)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync($"{server.Url}/apiV1/api.php?token={token}", 
                        client);
                var value = await httpResponseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ApiResult<bool>>(value);
                if (response.response_code.Equals(200))
                    return response.data;
                else return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
    public async Task<UpdateClientPasswordResponse?> UpdateClientPasswordAsync(Server server,UpdateClientPasswordDto client)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync($"{server.Url}/apiV1/api.php?token={token}", 
                        client);
                var value = await httpResponseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ApiResult<UpdateClientPasswordResponse>>(value);
                if (response.response_code.Equals(200))
                    return response.data;
                else return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
    public async Task<List<OnlineClient>> GetOnlineClientsAsync(Server server)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync($"{server.Url}/apiV1/api.php?token={token}",
                        new Request(Methods.getOnlineClients));
                var value = await httpResponseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ApiResult<List<OnlineClient>>>(value);
                if (response.response_code.Equals(200))
                    return response.data;
                else return new List<OnlineClient>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new List<OnlineClient>();
            }
        }
    }
}