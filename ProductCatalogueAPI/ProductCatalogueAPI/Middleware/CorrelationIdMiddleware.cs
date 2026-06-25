using System.Diagnostics;

namespace ProductCatalogueAPI.Middleware
{
    public class CorrelationIdMiddleware
    {
        private const string CorrelationIdHeader = "X-Correlation-ID";
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Use the client's correlation ID if provided
            // otherwise generate a new one
            var correlationId = context.Request.Headers[CorrelationIdHeader]
                                    .FirstOrDefault()
                                ?? Guid.NewGuid().ToString();

            // Store it so any service in the pipeline can access it
            context.Items["CorrelationId"] = correlationId;

            // Echo it back in the response header
            // so clients can log it on their side too
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationIdHeader] = correlationId;
                return Task.CompletedTask;
            });

            // Push into Serilog context so every log line
            // in this request automatically includes the ID
            using (Serilog.Context.LogContext.PushProperty(
                "CorrelationId", correlationId))
            {
                await _next(context);
            }
        }
}

}
