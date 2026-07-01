using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options; // Add this using directive
using ProductCatalogueAPI.AI.Configuration;
using ProductCatalogueAPI.AI.Services;

namespace ProductCatalogueAPI.AI.Extensions;

public static class AIServiceExtensions
{
    public static IServiceCollection AddAIServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AzureOpenAIOptions>(
      options => configuration.GetSection(AzureOpenAIOptions.SectionName).Bind(options));

        services.AddScoped<NaturalLanguageQueryService>();

        return services;
    }
}