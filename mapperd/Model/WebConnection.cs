using Mapper;

namespace mapperd.Model;

public record WebConnection
{
    /// <summary>
    /// Connection identifier
    /// </summary>
    public long Id { get; init; }
    
    /// <summary>
    /// Owned devices
    /// </summary>
    public List<Device> Devices { get; init; }
    
    /// <summary>
    /// Connection settings
    /// </summary>
    public ConnectionSettings Settings { get; init; }
}

public class ConnectionSettings
{
    /// <summary>
    /// Milliseconds between each poll
    /// </summary>
    public int PollingInterval { get; set; } = 50;
}