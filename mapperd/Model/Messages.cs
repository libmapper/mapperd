using System.Text.Json;
using System.Text.Json.Nodes;

namespace mapperd.Model;
/*
    Websocket Messages
*/

/// <summary>
/// Base
/// </summary>
public record Message
{
    /// <summary>
    /// Opcode
    /// </summary>
    public int Op { get; init; }
    
    /// <summary>
    /// Message (possibly null)
    /// </summary>
    public JsonNode Data { get; init; }
}


public enum OpCodes
{
    /*
      Client -> Daemon messages
     */
    Init = 0, // Reserve a new connection id. No data attached.
    Resume = 1, // Resume a connection. Data: Connection ID (ulong)
    
    /*
      Bidirectional messages
     */
    SignalData = 2, // Update signal value. Data: SignalData struct
    
    /*
        Daemon -> Client messages
     */
    ConnectionId = 3, // Connection ID. Sent in reply to an Init opcode. Data: Connection ID (ulong)
    Error = 4, // Error message. Data: Error message (string)
}

struct SignalData
{
    public ulong SignalId; // Signal ID
    public JsonNode Value; // Whatever value the signal has (number list for vectors, single number for scalars, etc)
}
