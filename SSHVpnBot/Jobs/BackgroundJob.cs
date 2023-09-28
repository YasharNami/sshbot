namespace SSHVpnBot.Jobs;

public class BackgroundJob
{
    public Task? _timerTask;
    public readonly PeriodicTimer _timer;
    public readonly CancellationTokenSource _cts = new();

    public BackgroundJob(TimeSpan interval)
    {
        _timer = new PeriodicTimer(interval);
    }

    public void Start()
    {
        _timerTask = DoWorkAsync();
    }

    public virtual async Task DoWorkAsync()
    {
        try
        {
            while (await _timer.WaitForNextTickAsync(_cts.Token))
            {
                // throw new NotImplementedException();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public async void StopAsync()
    {
        if (_timerTask is null) return;
        _cts.Cancel();
        await _timerTask;
        _cts.Dispose();
        Console.WriteLine("Task was cancelled.");
    }
}