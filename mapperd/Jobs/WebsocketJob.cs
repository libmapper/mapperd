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
    private CancellationTokenSource _cts = new();
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _thread = new Thread(Receive);
        _thread.IsBackground = true;
        _thread.Start();
        Console.WriteLine("Starting websocket job");
        return Task.CompletedTask;
    }

    private ArraySegment<Byte> buffer = new(new byte[8192]);
    
    private async void Receive()
    {
        while (_running)
        {
            // read messages from all connected sockets
            foreach (var socket in _manager.ConnectedSockets)
            {
                if (_manager.Outbox.TryGetValue(socket.ConnectionId, out var queue))
                {
                    foreach (var outgoing in queue)
                    {
                        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(outgoing));
                        await socket.Socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
                if (!socket.RecvTask.IsCompleted) continue;
                var result = await socket.RecvTask;
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    socket.CloseTask.SetResult();
                   // _manager.ConnectedSockets.Remove(socket);
                    continue;
                }
                
                var message = Encoding.UTF8.GetString(socket.RecvBuffer.Array, 0, result.Count);
                var msg = JsonSerializer.Deserialize<Message>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (msg.Op == OpCode.SignalData)
                {
                    var data = msg.Data.Deserialize<SignalData>();
                    if (_manager.Connections.TryGetValue(socket.ConnectionId, out var connection))
                    {
                        if (connection.Signals.TryGetValue(data.SignalIdLong, out var signal))
                        {
                            signal.SetValue(data.Value.Deserialize<float>());
                            Console.WriteLine($"Set signal value to {data.Value}");
                        }
                    }
                }
                
                // reset the buffer
                socket.RecvTask = socket.Socket.ReceiveAsync(socket.RecvBuffer, _cts.Token);

            }
            
            Thread.Sleep(1);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _running = false;
        Console.WriteLine("Stopping job");
        return _cts.CancelAsync();
    }
}