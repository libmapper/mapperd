using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
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
        _thread = new Thread(() => Receive());
        Console.WriteLine("Starting websocket job");
        return Task.CompletedTask;
    }

    private ArraySegment<Byte> buffer = new(new byte[8192]);
    
    private async void Receive()
    {
        while (_running)
        {
            // 
            
            Thread.Sleep(1);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _running = false;
        Console.WriteLine("Stopping job");
        return Task.CompletedTask;
    }
}