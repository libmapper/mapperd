using Mapper;

namespace mapperd.Model;

public record WebConnection(long id)
{
    /// <summary>
    /// Connection identifier
    /// </summary>
    public long Id { get; init; } = id;

    /// <summary>
    /// Owned devices
    /// </summary>
    public Dictionary<long, Device> Devices { get; init; } = new();

    /// <summary>
    /// Connection settings
    /// </summary>
    public ConnectionSettings Settings { get; init; } = new();
}

public class ConnectionSettings
{
    /// <summary>
    /// Milliseconds between each poll
    /// </summary>
    public int PollingInterval { get; set; } = 50;
}