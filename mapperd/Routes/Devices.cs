using IdGen;
using Mapper;
using mapperd.Model;
using mapperd.Util;
using Microsoft.AspNetCore.Mvc;

namespace mapperd.Routes;

[Route("/devices")]
[ApiController]
public class DevicesController(IdGenerator _idGen) : ControllerBase
{
    [HttpPost]
    public void Create([FromBody] DeviceCreateRequest request)
    {
        // Create device
        var dev = new Device(request.Name);
        // Add device to connection
        var conn = (WebConnection) HttpContext.Items["Connection"];
        conn.Devices.Add(_idGen.CreateId(), dev);
    }
}

public struct DeviceCreateRequest
{
    public string Name { get; set; }
}

public struct DeviceCreateResponse
{
    public bool Successful;
    public ulong DeviceId;
}