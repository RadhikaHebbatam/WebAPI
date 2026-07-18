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

        // Build the request body for the Azure AI Foundry Responses API
        // (the configured Endpoint already points at .../openai/v1/responses)
        var requestBody = new
        {
            model = _config.DeploymentName,
            input = new[]
            {
                new { type = "message", role = "system", content = systemPrompt },
                new { type = "message", role = "user", content = userQuery }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_config.Endpoint, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return new ProductQueryFilter(); // safe fallback

        // Parse the Responses API envelope: output[] contains items of type
        // "message", each with content[] items of type "output_text".
        using var doc = JsonDocument.Parse(responseBody);
        string? messageContent = null;
        foreach (var item in doc.RootElement.GetProperty("output").EnumerateArray())
        {
            if (item.GetProperty("type").GetString() != "message")
                continue;

            foreach (var part in item.GetProperty("content").EnumerateArray())
            {
                if (part.GetProperty("type").GetString() == "output_text")
                {
                    messageContent = part.GetProperty("text").GetString();
                    break;
                }
            }
        }

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