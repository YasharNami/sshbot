using ConnectBashBot.Telegram.Handlers;

namespace SSHVpnBot.Jobs;

public class ServerSyncer : BackgroundJob
{
    public ServerSyncer(TimeSpan interval) : base(interval)
    {
    }

    public override async Task DoWorkAsync()
    {
        try
        {
            while (await _timer.WaitForNextTickAsync(_cts.Token)) await MainHandler.SyncAllServers();
        }
        catch (OperationCanceledException)
        {
        }
    }
}