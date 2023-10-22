namespace SSHVpnBot.Services.Panel.Models;

public class PanelClientDto
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string Mobile { get; set; }
    public string Multiuser { get; set; }
    public DateTime StartDate { get; set; }
    public string? EndDate { get; set; }
    public string DateOneConnect { get; set; }
    public string CustomerUser { get; set; }
    public string Status { get; set; }
    public string Traffic { get; set; }
    public string Referral { get; set; }
    public string Desc { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ClientStats> Traffics { get; set; }
}

public class ClientStats
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Download { get; set; }
    public string Upload { get; set; }
    public string Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
public class CreateNewClientDto
{
    public string Token { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string Mobile { get; set; }
    public int Multiuser { get; set; }
    public int Traffic { get; set; }
    public string type_traffic { get; set; }
    public int connection_start { get; set; }

    public DateTime ExpDate { get; set; }
    public string Desc { get; set; }
}

public class UpdateClientDto
{
    public string Token { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string Mobile { get; set; }
    public int Multiuser { get; set; }
    public int Traffic { get; set; }
    public string TypeTraffic { get; set; }
    public string Activate { get; set; }
    public DateTime ExpDate { get; set; }
    public string Desc { get; set; }
}

public class DeleteClientDto
{
    public string Token { get; set; }

    public string Username { get; set; }
}

public class SuspendClientDto
{
    public string Token { get; set; }

    public string Username { get; set; }
}
public class UnSuspendClientDto
{
    public string Token { get; set; }

    public string Username { get; set; }
}
public class GetClientTrafficsDto
{
    public string method { get; set; }

    public string username { get; set; }
}

public class UpdateClientPasswordDto
{
    public string method { get; set; }

    public string username { get; set; }
    public string password { get; set; }
}
public class UpdateClientPasswordResponse
{
    public string id { get; set; }

    public string token { get; set; }

    public string description { get; set; }

    public string Allowips { get; set; }

    public string enable { get; set; }
    
    public string method { get; set; }

    public string username { get; set; }
    public string password { get; set; }
}

public class OnlineClient
{
    public string username { get; set; }

    public string ip { get; set; }
public string connection { get; set; }
public string pid { get; set; }
}

public class ExtendClientDto
{
    public string token { get; set; }
    public string username { get; set; }
    public string day_date { get; set; }
    public string re_date { get; set; }
    public string re_traffic { get; set; }
}