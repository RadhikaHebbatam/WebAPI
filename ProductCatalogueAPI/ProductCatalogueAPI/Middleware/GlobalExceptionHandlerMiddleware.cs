using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace ProductCatalogueAPI.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(
       HttpContext context,
       Exception exception)
        {
            var correlationId = context.Items["CorrelationId"]?.ToString()
                                ?? "unknown";

            // Log full detail internally — this goes to Serilog
            _logger.LogError(exception,
                "Unhandled exception. CorrelationId: {CorrelationId}, " +
                "Path: {Path}, Method: {Method}",
                correlationId,
                context.Request.Path,
                context.Request.Method);

            context.Response.ContentType = "application/json";

            // Map known exception types to HTTP status codes
            var (statusCode, message) = exception switch
            {
                ArgumentNullException => (HttpStatusCode.BadRequest,
                                                "A required value was missing."),
                ArgumentException => (HttpStatusCode.BadRequest,
                                                "Invalid argument provided."),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized,
                                                "You are not authorised."),
                KeyNotFoundException => (HttpStatusCode.NotFound,
                                                "The requested resource was not found."),
                InvalidOperationException => (HttpStatusCode.Conflict,
                                                "Operation is not valid for current state."),
                TimeoutException => (HttpStatusCode.GatewayTimeout,
                                                "The operation timed out."),
                _ => (HttpStatusCode.InternalServerError,
                                                "An unexpected error occurred.")
            };

            context.Response.StatusCode = (int)statusCode;

            var problemDetails = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = message,
                Instance = context.Request.Path
            };

            problemDetails.Extensions["correlationId"] = correlationId;
            problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

            // Only include stack trace in Development
            // NEVER expose this in Production
            if (_environment.IsDevelopment())
            {
                problemDetails.Extensions["detail"] = exception.Message;
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(problemDetails, options);
            await context.Response.WriteAsync(json);
        }

    }
}
