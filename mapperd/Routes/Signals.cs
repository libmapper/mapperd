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
        var conn = (MapperSession) HttpContext.Items["Connection"];
        if (!conn.Devices.TryGetValue(id, out var device))
        {
            return NotFound();
        }
        var sig = device.AddSignal(args.Direction, args.Name, args.VectorLength, args.Type, unit: args.Units);
        if (args.Min != null)
        {
            sig.SetProperty(Property.Min, args.Min);
        }
        if (args.Max != null)
        {
            sig.SetProperty(Property.Max, args.Max);
        }
        
        var sigId = _idGen.CreateId();
        conn.Signals.Add(sigId, new SignalSpec
        {
            Signal = sig,
            Type = args.Type
        });
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
        var conn = (MapperSession) HttpContext.Items["Connection"];
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
            Value = JsonValue.Create(signal.Signal.GetValue().Item1)
        });
    }
    
    [HttpDelete]
    [RequiresConnection]
    [Route("{signalId:long}")]
    public IActionResult Delete(long id, long signalId)
    {
        var conn = (MapperSession) HttpContext.Items["Connection"];
        if (!conn.Devices.TryGetValue(id, out var device))
        {
            return NotFound();
        }
        if (!conn.Signals.TryGetValue(signalId, out var signal))
        {
            return NotFound();
        }

        device.RemoveSignal(signal.Signal);
        conn.Signals.Remove(signalId);
        return NoContent();
    }
}

public struct CreateSignalArgs()
{
    public string Name { get; set; }
    public Signal.Direction Direction { get; set; }
    public MapperType Type { get; set; }

    public int VectorLength { get; set; } = 1;
    public float? Min { get; set; }
    public float? Max { get; set; }
    public string? Units { get; set; }
}

public struct CreateSignalResponse
{
    public bool Successful { get; set; }
    public string SignalId { get; set; }
}