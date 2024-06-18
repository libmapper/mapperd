using System.Net.WebSockets;
using IdGen;

namespace mapperd.Model;

public class ConnectionManager
{
    private IdGenerator IdGenerator { get; }
    public ConnectionManager(IdGenerator idGenerator)
    {
        IdGenerator = idGenerator;
    }
    
    public List<SocketMeta> PendingSockets { get; } = new();
    public Dictionary<long, WebConnection> Connections { get; } = new();

    public WebConnection ReserveConnection()
    {
        var con = new WebConnection
        {
            Id = IdGenerator.CreateId(),
            Devices = new(),
            Settings = new()
        };
        Connections.Add(con.Id, con);
        return con;
    }
}

public struct SocketMeta
{
    public WebSocket Socket;
    public Task<WebSocketReceiveResult> RecvTask;
    public ArraySegment<byte> RecvBuffer;
    public ulong? ConnectionId;
}