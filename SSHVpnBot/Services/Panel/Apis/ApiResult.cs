namespace SSHVpnBot.Services.Panel.Apis;

public class ApiResult<T>
{
    public T? data { get; set; }
    public int response_code { get; set; }
    public string response_desc { get; set; }
}

public class Request
{
    public Request(string method)
    {
        this.method = method;
    }
    public string method { get; set; }
}