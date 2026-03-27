using System.Text.Json;
using System.Text.Json.Serialization;

namespace OllamaAgentChat.Services;

public class OllamaModelService
{
    private readonly HttpClient _httpClient;
    private readonly string _ollamaBaseUrl;

    public OllamaModelService(string ollamaBaseUrl)
    {
        _httpClient = new HttpClient();
        _ollamaBaseUrl = ollamaBaseUrl.Replace("/v1", ""); // Rimuove /v1 se presente
    }

    public async Task<List<string>> GetAvailableModelsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_ollamaBaseUrl}/api/tags");
            
            if (!response.IsSuccessStatusCode)
                return new List<string>();

            var content = await response.Content.ReadAsStringAsync();
            var modelsResponse = JsonSerializer.Deserialize<OllamaModelsResponse>(content);

            return modelsResponse?.Models?
                .Select(m => m.Name)
                .OrderBy(n => n)
                .ToList() ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
}

public class OllamaModelsResponse
{
    [JsonPropertyName("models")]
    public List<OllamaModel>? Models { get; set; }
}

public class OllamaModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("modified_at")]
    public string ModifiedAt { get; set; } = string.Empty;
}
