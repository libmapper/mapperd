using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using mapperd.Model;

namespace mapperd.Jobs;

public class WebsocketJob(ConnectionManager _manager, JsonSerializerOptions _jOpts) : IHostedService
{

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
    
    private async void Receive()
    {
        while (_running)
        {
            _manager.LockOutbox();
            // read messages from all connected sockets
            foreach (var socket in _manager.ConnectedSockets)
            {
                
                if (_manager.Outbox.TryGetValue(socket.ConnectionId, out var queue))
                {
                    
                    List<ArraySegment<byte>> outgoingQueue = [];
                    foreach (var outgoing in queue.AsEnumerable().Reverse())
                    {
                        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(outgoing, _jOpts));
                        outgoingQueue.Add(new ArraySegment<byte>(bytes));
                    }
                    queue.Clear();
                    
                    foreach (var oMsg in outgoingQueue)
                    {
                        await socket.Socket.SendAsync(oMsg, WebSocketMessageType.Text, true, _cts.Token);
                    }
                }
                
                if (!socket.RecvTask.IsCompleted) continue;
                var result = await socket.RecvTask;
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    socket.CloseTask.SetResult();
                    continue;
                }
                
                var message = Encoding.UTF8.GetString(socket.RecvBuffer.Array, 0, result.Count);
                var msg = JsonSerializer.Deserialize<Message>(message, _jOpts);
                if (msg.Op == OpCode.SignalData)
                {
                    var data = msg.Data.Deserialize<SignalData>(_jOpts);
                    if (_manager.Sessions.TryGetValue(socket.ConnectionId, out var connection))
                    {
                        if (connection.Signals.TryGetValue(data.SignalId, out var signal))
                        {
                            if (data.Value is JsonArray)
                            {
                                signal.Signal.SetValue(data.Value.Deserialize(SignalSpec.ConvertMapperTypeToNative(signal.Type).MakeArrayType()));
                            }
                            else
                            {
                                signal.Signal.SetValue(data.Value.Deserialize(SignalSpec.ConvertMapperTypeToNative(signal.Type)));
                            }
                        }
                    }
                }
                
                // reset the buffer
                socket.RecvTask = socket.Socket.ReceiveAsync(socket.RecvBuffer, _cts.Token);

            }
            _manager.UnlockOutbox();

            // find orphaned sessions and remove

            TagOrphaned();
            DestroyOrphaned();
            
            // remove disconnected sockets
            _manager.ConnectedSockets.RemoveAll(socket => socket.Socket.State == WebSocketState.Closed);
            Thread.Sleep(10);
        }
    }

    /// <summary>
    /// Will tag any Sessions that have no connections claiming them and do not already have a destruction time set
    /// Will set a time three seconds in the future to destroy the session
    /// </summary>
    void TagOrphaned()
    {
        foreach (var session in _manager.Sessions)
        {
            if (_manager.ConnectedSockets.Find(socket => socket.ConnectionId == session.Key) == null)
            {
                if (session.Value.DestructionTime == null && session.Value.Settings.DestroyTimeout >= 0)
                {
                    session.Value.DestructionTime = DateTime.Now.AddSeconds(session.Value.Settings.DestroyTimeout);
                }
            } else if (session.Value.DestructionTime != null)
            {
                // session was resumed
                session.Value.DestructionTime = null;
            }
        }
    }
    
    [DllImport("mapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern void mpr_dev_free(IntPtr dev);
    
    void DestroyOrphaned()
    {
        var now = DateTime.Now;
        var toRemove = new List<string>();
        foreach (var session in _manager.Sessions)
        {
            if (session.Value.DestructionTime != null && session.Value.DestructionTime < now)
            {
                toRemove.Add(session.Key);
            }
        }

        foreach (var id in toRemove)
        {
            foreach (var dev in _manager.Sessions[id].Devices)
            {
                mpr_dev_free(dev.Value.NativePtr);
            }
            _manager.Sessions.Remove(id);
            Console.WriteLine($"Destroyed session {id}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _running = false;
        Console.WriteLine("Stopping job");
        return _cts.CancelAsync();
    }
}