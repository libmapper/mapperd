using System.Net.WebSockets;
using IdGen;

namespace mapperd.Model;

public class ConnectionManager
{

    public Dictionary<long, List<Message>> Outbox = new();    
    
    private IdGenerator IdGenerator { get; }
    public ConnectionManager(IdGenerator idGenerator)
    {
        IdGenerator = idGenerator;
    }
    
    public List<SocketMeta> ConnectedSockets { get; } = new();
    public Dictionary<long, WebConnection> Connections { get; } = new();

    public WebConnection ReserveConnection()
    {
        var con = new WebConnection(IdGenerator.CreateId());
        Connections.Add(con.Id, con);
        return con;
    }
    public void QueueOutgoingMessage(long id, Message message)
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

public struct SocketMeta
{
    public WebSocket Socket;
    public Task<WebSocketReceiveResult> RecvTask;
    public ArraySegment<byte> RecvBuffer;
    public long? ConnectionId;
}