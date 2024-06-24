using System.Net.WebSockets;
using IdGen;

namespace mapperd.Model;

public class ConnectionManager(IdGenerator idGenerator)
{

    public readonly Dictionary<long, List<Message>> Outbox = new();    
    
    private IdGenerator IdGenerator { get; } = idGenerator;

    public List<SocketMeta> ConnectedSockets { get; } = new();
    public Dictionary<long, MapperSession> Sessions { get; } = new();

    public MapperSession ReserveConnection()
    {
        var con = new MapperSession(IdGenerator.CreateId());
        Sessions.Add(con.Id, con);
        return con;
    }
    public void QueueOutgoingMessage(long id, Message message)
    {
        if (outLock)
        {
            outQueue.Add(new KeyValuePair<long, Message>(id, message));
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
    private List<KeyValuePair<long, Message>> outQueue = [];
    public void LockOutbox()
    {
        outLock = true;
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
        outLock = false;
    }
}

public class SocketMeta
{
    public WebSocket Socket;
    public Task<WebSocketReceiveResult> RecvTask;
    public ArraySegment<byte> RecvBuffer;
    public long ConnectionId;
    public TaskCompletionSource CloseTask;
}