using ProductCatalogueAPI.Middleware;

namespace ProductCatalogueAPI.Extensions;

/// <summary>
/// WHY: Extension methods keep Program.cs clean and readable.
/// Instead of registering every middleware individually in Program.cs,
/// one method call wires up the entire custom middleware stack.
/// Any new middleware gets added here — Program.cs never grows.
///
/// WHY order matters:
/// Middleware runs top to bottom on request, bottom to top on response.
/// Correlation ID must be FIRST — everything else needs the ID.
/// Exception handler must wrap everything below it to catch all errors.
/// Request logging goes after exception handler so errors are logged too.
/// Security headers go last — added to every outgoing response.
/// </summary>
public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseProductCatalogueMiddleware(
        this IApplicationBuilder app)
    {
        // 1. Correlation ID — must be first
        //    stamps every request with a unique traceable ID
        app.UseMiddleware<CorrelationIdMiddleware>();

        // 2. Exception handler — wraps everything below
        //    catches any unhandled exception in the pipeline
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

        // 3. Request logging — after exception handler
        //    logs both successful requests and errors
        app.UseMiddleware<RequestLoggingMiddleware>();

        // 4. Security headers — added to every response
        app.UseMiddleware<SecurityHeadersMiddleware>();

        return app;
    }
}