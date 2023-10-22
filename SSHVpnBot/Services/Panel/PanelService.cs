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
    protected readonly string token = "1697652817F0EDZA7UG3H6W51";
    
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
    public async Task<List<PanelClientDto>?> GetAllUsersAsync(Server server)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpResponseMessage =
                    await httpClient.GetAsync($"{server.Url}/api/{token}/listuser");
                var value = await httpResponseMessage.Content.ReadAsStringAsync();
                try
                {
                    var response = JsonConvert.DeserializeObject<List<PanelClientDto>>(value);
                    return response;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new List<PanelClientDto>();
            }
        }
    }
    public async Task<bool?> CreateNewClientAsync(Server server,CreateNewClientDto client)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                client.Token = token;
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync($"{server.Url}/api/adduser", 
                        client);
                var value = await httpResponseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ApiResult>(value);
                if (response.message.Equals("User Created"))
                {
                    return true;
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
    public async Task<ApiResult?> UpdateClientAsync(Server server,UpdateClientDto client)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                client.Token = token;
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync($"{server.Url}/api/edituser", 
                        client);
                var value = await httpResponseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ApiResult>(value);
                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
    public async Task<ApiResult?> DeleteClientAsync(Server server,DeleteClientDto client)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                client.Token = token;
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync($"{server.Url}/api/delete", 
                        client);
                var value = await httpResponseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ApiResult>(value);
                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
    public async Task<ApiResult?> SuspendClientAsync(Server server,SuspendClientDto client)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                client.Token = token;
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync($"{server.Url}/api/deactive", 
                        client);
                var value = await httpResponseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ApiResult>(value);
                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
    public async Task<ApiResult?> UnSuspendClientAsync(Server server,UnSuspendClientDto client)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                client.Token = token;
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync($"{server.Url}/api/active", 
                        client);
                var value = await httpResponseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ApiResult>(value);
                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
    
    public async Task<ApiResult?> ExtendClientAsync(Server server,ExtendClientDto client)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                client.token = token;
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync($"{server.Url}/api/renewal", 
                        client);
                var value = await httpResponseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ApiResult>(value);
                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
    // public async Task<bool> GetClientTrafficAsync(Server server,GetClientTrafficsDto client)
    // {
    //     using (var httpClient = new HttpClient())
    //     {
    //         try
    //         {
    //             httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    //             var httpResponseMessage =
    //                 await httpClient.PostAsJsonAsync($"{server.Url}/apiV1/api.php?token={token}", 
    //                     client);
    //             var value = await httpResponseMessage.Content.ReadAsStringAsync();
    //             var response = JsonConvert.DeserializeObject<ApiResult<bool>>(value);
    //             return response.data;
    //         }
    //         catch (Exception e)
    //         {
    //             Console.WriteLine(e.Message);
    //             return false;
    //         }
    //     }
    // }
  
    public async Task<List<OnlineClient>?> GetOnlineClientsAsync(Server server)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpResponseMessage =
                    await httpClient.GetAsync($"{server.Url}/api/{token}/online");
                var value = await httpResponseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<List<OnlineClient>?>(value);
                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new List<OnlineClient>();
            }
        }
    }
}