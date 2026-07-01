using Microsoft.Extensions.Options;
using ProductCatalogueAPI.AI.Configuration;
using ProductCatalogueAPI.Core.Common;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ProductCatalogueAPI.AI.Services;

public class NaturalLanguageQueryService
{
    private readonly AzureOpenAIOptions _config;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public NaturalLanguageQueryService(IOptions<AzureOpenAIOptions> options)
    {
        _config = options.Value;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("api-key", _config.ApiKey);
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<ProductQueryFilter> ParseQueryAsync(string userQuery)
    {
        var systemPrompt = """
            You are a product search assistant.
            Convert the user's natural language query into a JSON filter object.
            Return ONLY valid JSON with no explanation, no markdown, no code blocks.
            The JSON must match this exact structure (all fields are optional):
            {
                "MinPrice": <decimal or null>,
                "MaxPrice": <decimal or null>,
                "MinStock": <integer or null>,
                "MaxStock": <integer or null>,
                "NameContains": "<string or null>",
                "CategoryId": <integer or null>,
                "IsActive": <boolean or null>,
                "OrderBy": "<Price|Name|StockQuantity or null>",
                "OrderDirection": "<ASC|DESC or null>"
            }
            Examples:
            User: "show me products under $50"
            Response: {"MaxPrice": 50}
            User: "cheap keyboards with low stock"
            Response: {"MaxPrice": 100, "NameContains": "keyboard", "MaxStock": 20}
            User: "all active products sorted by price"
            Response: {"IsActive": true, "OrderBy": "Price", "OrderDirection": "ASC"}
            """;

        // Build the request body manually
        var requestBody = new
        {
            model = _config.DeploymentName,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userQuery }
            },
            temperature = 0.1 // low temperature = more deterministic/consistent JSON output
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Foundry endpoint uses /v1/chat/completions path
        var url = $"{_config.Endpoint.TrimEnd('/')}/v1/chat/completions";

        var response = await _httpClient.PostAsync(url, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return new ProductQueryFilter(); // safe fallback

        // Parse the OpenAI response envelope to get the message content
        using var doc = JsonDocument.Parse(responseBody);
        var messageContent = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(messageContent))
            return new ProductQueryFilter();

        try
        {
            return JsonSerializer.Deserialize<ProductQueryFilter>(
                messageContent, _jsonOptions) ?? new ProductQueryFilter();
        }
        catch (JsonException)
        {
            return new ProductQueryFilter();
        }
    }
}