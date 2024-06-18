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
        _thread = new Thread(() => Recieve());
        return Task.CompletedTask;
    }

    private ArraySegment<Byte> buffer = new(new byte[8192]);
    
    private async void Recieve()
    {
        while (_running)
        {
            // Check pending connections
            foreach (var pending in _manager.PendingSockets) 
            {
                if (pending.RecvTask.IsCompleted)
                {
                    var result = await pending.RecvTask;
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _manager.PendingSockets.Remove(pending);
                        continue;
                    }
                    var message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                    var msg = JsonSerializer.Deserialize<Message>(message);
                    if (msg.Op == OpCode.Init)
                    {
                        var con = _manager.ReserveConnection();
                        var response = new Message
                        {
                            Op = OpCode.ConnectionId,
                            Data = JsonValue.Create(con.Id)
                        };
                        var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
                        await pending.Socket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, 
                            true, CancellationToken.None);
                        _manager.PendingSockets.Remove(pending);
                        _manager.Connections.Add(con.Id, con);
                    }
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