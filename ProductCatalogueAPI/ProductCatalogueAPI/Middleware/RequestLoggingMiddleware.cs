using System.Diagnostics;

namespace ProductCatalogueAPI.Middleware;

/// <summary>
/// WHY: Every request and response should be logged in production.
/// This gives you visibility into traffic patterns, slow endpoints,
/// error rates, and client behaviour — all feeding into Kibana later.
/// We log duration so we can alert on slow endpoints.
/// We skip health check paths to reduce noise in logs —
/// load balancers ping /health every 30 seconds.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    // Paths we deliberately skip to reduce log noise
    private static readonly string[] SkippedPaths =
    [
        "/health",
        "/health/live",
        "/health/ready",
        "/favicon.ico"
    ];

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Skip noisy paths
        if (SkippedPaths.Any(p =>
                path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var correlationId = context.Items["CorrelationId"]?.ToString();
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "HTTP {Method} {Path} started. CorrelationId: {CorrelationId}",
            context.Request.Method,
            path,
            correlationId);

        await _next(context);

        stopwatch.Stop();

        // WHY different log levels based on status code:
        // 5xx errors need immediate attention — log as Error
        // 4xx are client mistakes — log as Warning
        // 2xx are success — log as Information
        // This lets you filter Kibana by severity instantly
        var logLevel = context.Response.StatusCode >= 500
            ? LogLevel.Error
            : context.Response.StatusCode >= 400
                ? LogLevel.Warning
                : LogLevel.Information;

        _logger.Log(logLevel,
            "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms. " +
            "CorrelationId: {CorrelationId}",
            context.Request.Method,
            path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            correlationId);
    }
}