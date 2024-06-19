using Mapper;

namespace mapperd.Model;

public record MapperSession(long _id)
{
    /// <summary>
    /// Connection identifier
    /// </summary>
    public long Id { get; init; } = _id;

    /// <summary>
    /// Owned devices
    /// </summary>
    public Dictionary<long, Device> Devices { get; init; } = new();

    /// <summary>
    /// Signals
    /// </summary>
    public Dictionary<long, Signal> Signals { get; init; } = new();

    /// <summary>
    /// Connection settings
    /// </summary>
    public ConnectionSettings Settings { get; init; } = new();

    /// <summary>
    /// When the session will be destroyed
    /// </summary>
    public DateTimeOffset? DestructionTime { get; set; } = null;
}

public class ConnectionSettings
{
    /// <summary>
    /// Milliseconds between each poll
    /// </summary>
    public int PollingInterval { get; set; } = 50;
    /// <summary>
    /// Time in seconds before the session is destroyed after being orphaned
    /// Set to -1 to make session live forever until destroyed manually
    /// </summary>
    public int DestroyTimeout { get; set; } = 3;
}