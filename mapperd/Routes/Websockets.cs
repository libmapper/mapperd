using mapperd.Model;
using Microsoft.AspNetCore.Mvc;

namespace mapperd.Routes;

[Route("/ws")]
public class WebsocketController(ConnectionManager mgr) : ControllerBase
{
    private ConnectionManager _manager = mgr;

    public async Task Get()
    {
        using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        var buffer = new ArraySegment<byte>(new byte[1024]);
        var result = socket.ReceiveAsync(buffer, CancellationToken.None);
        var socketMeta = new SocketMeta
        {
            Socket = socket,
            RecvTask = result,
            RecvBuffer = buffer
        };
        _manager.PendingSockets.Add(socketMeta);
        
    }
}