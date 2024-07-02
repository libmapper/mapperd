using System.Text.Json.Nodes;
using IdGen;
using Mapper;
using mapperd.Model;
using mapperd.Util;
using Microsoft.AspNetCore.Mvc;
using NanoidDotNet;

namespace mapperd.Routes;

[Route("/devices/{id}/signals")]
[ApiController]
public class Signals(ConnectionManager _mgr) : ControllerBase
{
    [HttpPost]
    [RequiresConnection]
    public IActionResult Create(string id, [FromBody] CreateSignalArgs args)
    {
        // check that the device exists
        var conn = (MapperSession) HttpContext.Items["Connection"];
        if (!conn.Devices.TryGetValue(id, out var device))
        {
            return NotFound();
        }
        var sig = device.AddSignal(args.Direction, args.Name, args.VectorLength, (MapperType)args.Type, unit: args.Units);
        if (args.Min != null)
        {
            sig.SetProperty(Property.Min, args.Min);
        }
        if (args.Max != null)
        {
            sig.SetProperty(Property.Max, args.Max);
        }

        var sigId = Nanoid.Generate();
        conn.Signals.Add(sigId, new SignalSpec
        {
            Signal = sig,
            Type = (MapperType)args.Type
        });
        return Ok(new CreateSignalResponse
        {
            SignalId = sigId.ToString(),
            Successful = true
        });
    }
    
    [Route("{signalId}")]
    [HttpGet]
    [RequiresConnection]
    public IActionResult Get(string id, string signalId)
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
    [Route("{signalId}")]
    public IActionResult Delete(string id, string signalId)
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
    public ApiCreateType Type { get; set; }

    public int VectorLength { get; set; } = 1;
    public float? Min { get; set; }
    public float? Max { get; set; }
    public string? Units { get; set; }
}

public enum ApiCreateType
{
    Double = MapperType.Double,
    Int32 = MapperType.Int32
}

public struct CreateSignalResponse
{
    public bool Successful { get; set; }
    public string SignalId { get; set; }
}