using IdGen;
using Mapper;
using mapperd.Model;
using mapperd.Util;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace mapperd.Routes;

[Route("/devices")]
[ApiController]
public class DevicesController(IdGenerator _idGen, Graph _graph) : ControllerBase
{
    
    [HttpPost]
    [RequiresConnection]
    public DeviceCreateResponse Create([FromBody] DeviceCreateRequest request)
    {
        // Create device
        var dev = new Device(request.Name, _graph);
        // Add device to connection
        var conn = (MapperSession) HttpContext.Items["Connection"];
        var id = _idGen.CreateId();
        conn.Devices.Add(id, dev);
        return new DeviceCreateResponse
        {
            Successful = true,
            DeviceId = id.ToString()
        };
    }
}

public struct DeviceCreateRequest
{
    public string Name { get; set; }
}

public struct DeviceCreateResponse
{
    public bool Successful { get; set; }
    public string DeviceId { get; set; }
}