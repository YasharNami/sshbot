namespace SSHVpnBot.Services.Panel.Apis;

public class ApiResult
{
    public string message { get; set; }
}

public class ApiResult<T>
{
    public T data { get; set; }
}

public class Request
{
    public Request(string method)
    {
        this.method = method;
    }
    public string method { get; set; }
}