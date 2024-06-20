using Mapper;
using Microsoft.AspNetCore.Mvc;

namespace mapperd.Routes;

/// <summary>
/// Query graph objects
/// </summary>
[Route("/objects/maps")]
[ApiController]
public class Maps(Graph _graph) : ControllerBase
{
    // list all maps on the graph
    [HttpGet]
    public List<ApiMap> List()
    {
        var maps = _graph.Maps;
        List<ApiMap> apiMaps = new();
        foreach (var map in maps)
        {
            apiMaps.Add(ToApiMap((Map)map));
        }

        return apiMaps;
    }
    
    
    private ApiMap ToApiMap(Map map)
    {
        var list = new List<string>();
        foreach (var signal in map.GetSignals())
        {
            list.Add(((Signal) signal).Id.ToString());
        }
        return new ApiMap
        {
            Id = map.Id.ToString(),
            Expression = (string)map.GetProperty(Property.Expression),
            Signals = list
        };
    }
}

public struct ApiMap
{
    public string Id { get; set; }
    public string Expression { get; set; }
    public List<String> Signals { get; set; }
}