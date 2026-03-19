namespace McpExample.API.Middleware;

/// <summary>
/// Middleware that enforces API Key authentication for the MCP endpoints.
/// Clients must include the header: X-Api-Key: {key}
/// </summary>
public class McpApiKeyMiddleware
{
    private const string ApiKeyHeader = "X-Api-Key";
    private const string McpPathPrefix = "/mcp";

    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<McpApiKeyMiddleware> _logger;

    public McpApiKeyMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<McpApiKeyMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments(McpPathPrefix, StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var extractedApiKey))
        {
            _logger.LogWarning("MCP request rejected: missing {Header} header.", ApiKeyHeader);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "API key is missing. Provide it via the X-Api-Key header." });
            return;
        }

        var configuredApiKey = _configuration["McpSettings:ApiKey"];
        if (string.IsNullOrWhiteSpace(configuredApiKey) || !string.Equals(extractedApiKey, configuredApiKey, StringComparison.Ordinal))
        {
            _logger.LogWarning("MCP request rejected: invalid API key.");
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API key." });
            return;
        }

        await _next(context);
    }
}
