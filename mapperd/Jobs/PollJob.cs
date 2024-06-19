using Mapper;
using mapperd.Model;

namespace mapperd.Routes;

public class PollJob(ConnectionManager _mgr, Graph _graph) : IHostedService
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
            _graph.Poll();
            foreach (var session in _mgr.Sessions)
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