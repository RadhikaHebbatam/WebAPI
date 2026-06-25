using Microsoft.Extensions.DependencyInjection;
using ProductCatalogueAPI.Core.Interfaces.Repositories;
using ProductCatalogueAPI.Infrastructure.Data;
using ProductCatalogueAPI.Infrastructure.Repositories;

namespace ProductCatalogueAPI.Infrastructure.Extensions
{
    /// <summary>
    /// WHY an extension method for registration:
    /// The API project needs to register all Infrastructure dependencies.
    /// But the API project should not need to know about every single
    /// class inside Infrastructure — that breaks encapsulation.
    /// This extension method is the ONLY thing the API project calls.
    /// Infrastructure decides what gets registered and how.
    /// 
    /// This is the standard .NET pattern — you see it everywhere:
    /// builder.Services.AddDbContext(...)
    /// builder.Services.AddAuthentication(...)
    /// We follow the same convention for our own layers.
    /// </summary>
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services)
        {
            // WHY Singleton for DbConnectionFactory:
            // It holds no state — just reads the connection string once.
            // Safe to share across all requests.
            // Creating a new factory per request would wastefully
            // re-read configuration on every single request.
            services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

            // WHY Scoped for Repositories:
            // Each repository creates a new connection per method call.
            // Scoped means one repository instance per HTTP request.
            // This is the correct lifetime for data access services —
            // it aligns with the unit-of-work pattern where all operations
            // in one request share the same logical scope.
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();

            return services;
        }
    }
    }
