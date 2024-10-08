using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using mapperd.Model;
using Microsoft.AspNetCore.Mvc;

namespace mapperd.Routes;

[Route("/ws")]
public class WebsocketController(ConnectionManager mgr, JsonSerializerOptions _jOpts) : ControllerBase
{
    [HttpGet]
    public async Task Get()
    {
        using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        var buffer = new ArraySegment<byte>(new byte[1024]);
        var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
        var message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
        Console.WriteLine(message);
        Message msg;
        try
        {
            msg = JsonSerializer.Deserialize<Message>(message, _jOpts);
        }
        catch
        {
            msg = new Message { Op = OpCode.Error, Data = null };
        }
        
        if (msg.Op == OpCode.Init)
        {
            // create session
            var con = mgr.ReserveConnection();
            var response = new Message
            {
                Op = OpCode.ConnectionId,
                Data = JsonValue.Create(con.Id)
            };
            var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response, _jOpts));
            await socket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            var closeSource = new TaskCompletionSource();
            var meta = new SocketMeta
            {
                ConnectionId = con.Id,
                RecvBuffer = buffer,
                RecvTask = socket.ReceiveAsync(buffer, CancellationToken.None),
                Socket = socket,
                CloseTask = closeSource
            };
            mgr.QueueAdd(meta);
            await closeSource.Task;
        }
        else
        { 
            // invalid first opcode
            var response = new Message
            {
                Op = OpCode.Error,
                Data = JsonValue.Create((int)ErrorReason.NoSession)
            };
            var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response, _jOpts));
            await socket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            await socket.CloseAsync(WebSocketCloseStatus.ProtocolError, "Invalid opcode", CancellationToken.None);
        }
    }
}