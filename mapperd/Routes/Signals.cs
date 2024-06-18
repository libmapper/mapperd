using IdGen;
using Mapper;
using mapperd.Model;
using mapperd.Util;
using Microsoft.AspNetCore.Mvc;

namespace mapperd.Routes;

[Route("/devices/{id}/signals")]
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
    
        var sig = device.AddSignal(args.Type, args.Name, 1, MapperType.Float);
        var sigId = _idGen.CreateId();
        conn.Signals.Add(sigId, sig);
        return Ok(new CreateSignalResponse
        {
            SignalId = sigId,
            Successful = true
        });

    }
}

public struct CreateSignalArgs
{
    public string Name { get; set; }
    public Signal.Direction Type { get; set; }
}

public struct CreateSignalResponse
{
    public bool Successful { get; set; }
    public long SignalId { get; set; }
}