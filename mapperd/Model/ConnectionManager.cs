using System.Net.WebSockets;
using IdGen;

namespace mapperd.Model;

public class ConnectionManager
{

    public Dictionary<long, List<Message>> Outbox = new();    
    
    public Mutex OutboxLock = new();
    
    private IdGenerator IdGenerator { get; }
    public ConnectionManager(IdGenerator idGenerator)
    {
        IdGenerator = idGenerator;
    }
    
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
        lock (this)
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
}

public class SocketMeta
{
    public WebSocket Socket;
    public Task<WebSocketReceiveResult> RecvTask;
    public ArraySegment<byte> RecvBuffer;
    public long ConnectionId;
    public TaskCompletionSource CloseTask;
}