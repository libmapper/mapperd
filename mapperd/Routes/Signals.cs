using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
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
        var sig = device.AddSignal(args.Direction, args.Name, args.VectorLength, args.NativeType, unit: args.Units);
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
            SignalId = sigId,
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
            SignalId = signalId,
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
    
    [JsonIgnore]
    internal MapperType NativeType => Type switch
    {
        ApiCreateType.Double => MapperType.Double,
        ApiCreateType.Int32 => MapperType.Int32,
        _ => throw new ArgumentOutOfRangeException()
    };

    public int VectorLength { get; set; } = 1;
    public float? Min { get; set; }
    public float? Max { get; set; }
    public string? Units { get; set; }
}

public enum ApiCreateType
{
    Double = 0,
    Int32 = 1
}

public struct CreateSignalResponse
{
    public bool Successful { get; set; }
    public string SignalId { get; set; }
}