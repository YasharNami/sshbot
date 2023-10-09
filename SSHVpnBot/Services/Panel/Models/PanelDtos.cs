namespace SSHVpnBot.Services.Panel.Models;

public class PanelClientDto
{
    public string id { get; set; }

    public string username { get; set; }

    public string password { get; set; }

    public string email { get; set; }

    public string mobile { get; set; }

    public string multiuser { get; set; }

    public DateTime startdate { get; set; }

    public DateTime finishdate { get; set; }

    public string enable { get; set; }

    public string traffic { get; set; }

    public string referral { get; set; }

    public string info { get; set; }

    public string days { get; set; }
    public string telegramid { get; set; }
    //public string userport { get; set; }
    //public string usersub { get; set; }
}

public class CreateNewClientDto
{
    public string method { get; set; }

    public string username { get; set; }

    public string password { get; set; }

    public string email { get; set; }

    public string mobile { get; set; }
    public int multiuser { get; set; }

    public string finishdate { get; set; }

    public int traffic { get; set; }

    public string referral { get; set; }
}

public class UpdateClientDto
{
    public string method { get; set; }

    public string username { get; set; }

    public string password { get; set; }

    public string email { get; set; }

    public string mobile { get; set; }
    public int multiuser { get; set; }

    public string finishdate { get; set; }

    public int traffic { get; set; }

    public string referral { get; set; }
}

public class DeleteClientDto
{
    public string method { get; set; }

    public string username { get; set; }
}

public class SuspendClientDto
{
    public string method { get; set; }

    public string username { get; set; }
}
public class UnSuspendClientDto
{
    public string method { get; set; }

    public string username { get; set; }
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

    public string pid { get; set; }
}