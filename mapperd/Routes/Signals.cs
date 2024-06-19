using System.Text.Json.Nodes;
using IdGen;
using Mapper;
using mapperd.Model;
using mapperd.Util;
using Microsoft.AspNetCore.Mvc;

namespace mapperd.Routes;

[Route("/devices/{id:long}/signals")]
[ApiController]
public class Signals(ConnectionManager _mgr, IdGenerator _idGen) : ControllerBase
{
    [HttpPost]
    [RequiresConnection]
    public IActionResult Create(long id, [FromBody] CreateSignalArgs args)
    {
        // check that the device exists
        var conn = (WebConnection) HttpContext.Items["Connection"];
        if (!conn.Devices.TryGetValue(id, out var device))
        {
            return NotFound();
        }
        var sig = device.AddSignal(args.Direction, args.Name, 1, MapperType.Float);
        var sigId = _idGen.CreateId();
        conn.Signals.Add(sigId, sig);
        return Ok(new CreateSignalResponse
        {
            SignalId = sigId.ToString(),
            Successful = true
        });
    }
    
    [Route("{signalId:long}")]
    [HttpGet]
    [RequiresConnection]
    public IActionResult Get(long id, long signalId)
    {
        var conn = (WebConnection) HttpContext.Items["Connection"];
        if (!conn.Devices.TryGetValue(id, out var device))
        {
            return NotFound();
        }
        if (!conn.Signals.TryGetValue(signalId, out var signal))
        {
            return NotFound();
        }
        return Ok(new SignalData
        {
            SignalId = signalId.ToString(),
            Value = JsonValue.Create(signal.GetValue().Item1)
        });
    }
    
    [HttpDelete]
    [RequiresConnection]
    [Route("{signalId:long}")]
    public IActionResult Delete(long id, long signalId)
    {
        var conn = (WebConnection) HttpContext.Items["Connection"];
        if (!conn.Devices.TryGetValue(id, out var device))
        {
            return NotFound();
        }
        if (!conn.Signals.TryGetValue(signalId, out var signal))
        {
            return NotFound();
        }

        device.RemoveSignal(signal);
        conn.Signals.Remove(signalId);
        return NoContent();
    }
}

public struct CreateSignalArgs
{
    public string Name { get; set; }
    public Signal.Direction Direction { get; set; }
}

public struct CreateSignalResponse
{
    public bool Successful { get; set; }
    public string SignalId { get; set; }
}