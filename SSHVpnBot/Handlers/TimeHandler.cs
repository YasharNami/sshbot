using SSHVpnBot;

namespace ConnectBashBot.Telegram.Handlers;

public class TimeHandler
{
    public static void ReqManager(object? state)
    {
        try
        {
            Program.Req = 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}