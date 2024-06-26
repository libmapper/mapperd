using System.Collections.Concurrent;
using System.Net.WebSockets;
using IdGen;
using NanoidDotNet;

namespace mapperd.Model;

public class ConnectionManager()
{

    public readonly Dictionary<string, List<Message>> Outbox = new();    
    public List<SocketMeta> ConnectedSockets { get; } = new();
    public Dictionary<string, MapperSession> Sessions { get; } = new();

    public MapperSession ReserveConnection()
    {
        var con = new MapperSession(Nanoid.Generate());
        Sessions.Add(con.Id, con);
        return con;
    }
    public void QueueOutgoingMessage(string id, Message message)
    {
        if (outLock)
        {
            outQueue.Add(new KeyValuePair<string, Message>(id, message));
        }
        else
        {
            if (Outbox.TryGetValue(id, out List<Message>? value))
            {
                value.Add(message);
            }
            else
            {
                Outbox.Add(id, [message]);
            }
        }
    }

    private bool outLock = false;
    private List<KeyValuePair<string, Message>> outQueue = [];
    public void LockOutbox()
    {
        outLock = true;
    }

    private List<SocketMeta> metaQueue = [];
    public void QueueAdd(SocketMeta meta)
    {
        lock (metaQueue)
        {
            metaQueue.Add(meta);
        }
    }
    

    public void UnlockOutbox()
    {
        // flush the queue
        foreach (var msg in outQueue)
        {
            if (Outbox.TryGetValue(msg.Key, out List<Message>? value))
            {
                value.Add(msg.Value);
            }
            else
            {
                Outbox.Add(msg.Key, [msg.Value]);
            }
        }

        if (outQueue.Count > 4)
        {
            Console.WriteLine($"Overflow: {outQueue.Count} messages in queue");
        }

        outQueue.Clear();
        lock (metaQueue)
        {
            foreach (var meta in metaQueue)
            {
                ConnectedSockets.Add(meta);
            }
            metaQueue.Clear();
        }
        outLock = false;
    }
}

public class SocketMeta
{
    public WebSocket Socket;
    public Task<WebSocketReceiveResult> RecvTask;
    public ArraySegment<byte> RecvBuffer;
    public string ConnectionId;
    public TaskCompletionSource CloseTask;
}