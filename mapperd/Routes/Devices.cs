using Mapper;
using mapperd.Model;
using mapperd.Util;
using Microsoft.AspNetCore.Mvc;
using NanoidDotNet;

namespace mapperd.Routes;

[Route("/devices")]
[ApiController]
public class DevicesController(Graph _graph) : ControllerBase
{
    
    [HttpPost]
    [RequiresConnection]
    public DeviceCreateResponse Create([FromBody] DeviceCreateRequest request)
    {
        // Create device
        var dev = new Device(request.Name, _graph);
        // Add device to connection
        var conn = (MapperSession) HttpContext.Items["Connection"];
        var id = Nanoid.Generate();
        conn.Devices.Add(id, dev);
        return new DeviceCreateResponse
        {
            Successful = true,
            DeviceId = id
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