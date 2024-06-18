using mapperd.Model;

namespace mapperd.Util;

public class SnowflakeLookupMiddleware
{
    private ConnectionManager _connectionManager;
    public SnowflakeLookupMiddleware(RequestDelegate next, ConnectionManager connectionManager)
    {
        _next = next;
        _connectionManager = connectionManager;
    }

    private readonly RequestDelegate _next;

    public async Task InvokeAsync(HttpContext context)
    {
        var snowflake = context.Request.Headers["Session-ID"];
        if (snowflake.Count > 0)
        {
            var snowflakeId = snowflake[0];
            if (long.TryParse(snowflakeId, out var id))
            {
                if (_connectionManager.Connections.ContainsKey(id))
                {
                    var connection = _connectionManager.Connections[id];
                    context.Items["Connection"] = connection;
                }
            }
        }

        await _next(context);
    }
}