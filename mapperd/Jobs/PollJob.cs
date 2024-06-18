using mapperd.Model;

namespace mapperd.Routes;

public class PollJob(ConnectionManager _mgr) : IHostedService
{
    private bool _running = true;
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var thread = new Thread(Run)
        {
            IsBackground = true
        };
        thread.Start();
        return Task.CompletedTask;
    }

    void Run()
    {
        while (_running)
        {
            foreach (var session in _mgr.Connections)
            {
                foreach (var device in session.Value.Devices)
                {
                    device.Value
                        .Poll();
                }
            }
            Thread.Sleep(10);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _running = false;
        return Task.CompletedTask;
    }
}