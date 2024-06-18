using mapperd.Model;

namespace mapperd.Routes;

public class WebsocketJob : IHostedService
{
    private ConnectionManager _manager;
    public WebsocketJob(ConnectionManager mgr)
    {
        _manager = mgr;
    }

    private Thread _thread;
    private bool _running = true;
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _thread = new Thread(() => Recieve());
        return Task.CompletedTask;
    }

    private ArraySegment<Byte> buffer = new ArraySegment<byte>(new byte[8192]);
    private async void Recieve()
    {
        while (_running)
        {
            // Check pending connections
            if (_manager.PendingSockets.Count > 0)
            {
                var socket = _manager.PendingSockets[0];
                // recieve a message if there's one waiting
                var token = new CancellationToken();
                var result = socket.Socket.ReceiveAsync(buffer, token);
                if (!result.IsCompleted)
                {
                    
                }
            }
            
            Thread.Sleep(1);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _running = false;
        return Task.CompletedTask;
    }
}