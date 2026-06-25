using ProductCatalogueAPI.Application.Services;
using ProductCatalogueAPI.Core.Interfaces.Services;
using ProductCatalogueAPI.Infrastructure.Extensions;

namespace ProductCatalogueAPI.Extensions;

/// <summary>
/// WHY: All service registrations organised in one place.
/// Program.cs calls one method — this class handles everything.
/// As the app grows, new registrations go here not in Program.cs.
/// Each private method registers one concern — single responsibility.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProductCatalogueServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApiServices();
        services.AddApplicationServices();
        services.AddInfrastructure();
        services.AddCorsPolicy(configuration);
        services.AddHealthCheckServices();

        return services;
    }

    private static IServiceCollection AddApiServices(
        this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            // WHY: Returns 406 Not Acceptable if client requests
            // a format your API does not support.
            // Enforces that your API only returns what it promises.
            options.ReturnHttpNotAcceptable = true;
        })
        .AddJsonOptions(options =>
        {
            // WHY camelCase: JavaScript and TypeScript clients
            // expect camelCase. This prevents mismatches between
            // C# PascalCase properties and JS camelCase.
            options.JsonSerializerOptions.PropertyNamingPolicy =
                System.Text.Json.JsonNamingPolicy.CamelCase;

            // WHY: Do not serialise null values.
            // Keeps responses clean and reduces payload size.
            options.JsonSerializerOptions.DefaultIgnoreCondition =
                System.Text.Json.Serialization
                    .JsonIgnoreCondition.WhenWritingNull;
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = "Product Catalogue API",
                Version = "v1",
                Description = "Production-ready Product Catalogue REST API"
            });
        });

        return services;
    }

    private static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // WHY Scoped for services:
        // One instance per HTTP request.
        // Services hold no cross-request state —
        // but they coordinate work within a single request.
        services.AddScoped<IProductService, ProductService>();

        return services;
    }

    private static IServiceCollection AddCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy("ProductCataloguePolicy", policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
                }
                else
                {
                    // WHY this fallback only for development:
                    // Never ship AllowAnyOrigin to production.
                    // It allows any website to call your API.
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                }
            });
        });

        return services;
    }

    private static IServiceCollection AddHealthCheckServices(
        this IServiceCollection services)
    {
        // WHY health checks:
        // Load balancers, Kubernetes, and Octopus Deploy all
        // ping /health to know if your API is alive before
        // routing traffic to it.
        // Without this a crashed API still receives traffic.
        services.AddHealthChecks();

        return services;
    }
}