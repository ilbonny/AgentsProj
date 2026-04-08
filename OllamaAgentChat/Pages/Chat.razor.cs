using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OllamaAgentChat.Models;
using OllamaAgentChat.Services;

namespace OllamaAgentChat.Pages;

public partial class Chat : ComponentBase
{
    [Inject]
    private IConfiguration Configuration { get; set; } = default!;

    private IAgentService? _agentService;
    private OllamaModelService? modelService;
    private List<ChatMessage> messages = new();
    private List<string> availableModels = new();
    private string selectedModel = string.Empty;
    private string userInput = string.Empty;
    private bool isLoading = false;
    private bool isDarkTheme = true;

    protected override async Task OnInitializedAsync()
    {
        var ollamaEndpoint = Configuration["Ollama:Endpoint"] ?? "http://localhost:11434/";
        var modelName = Configuration["Ollama:ModelName"] ?? "llama3.1:8b";

        //_agentService = new AzureAgentService(ollamaEndpoint, modelName);
        //_agentService = new OllamaAgentService(ollamaEndpoint, modelName);
        _agentService = new SequentialWeatherAgentService(ollamaEndpoint, modelName);

        modelService = new OllamaModelService(ollamaEndpoint);

        availableModels = await modelService.GetAvailableModelsAsync();
        
        if (availableModels.Count == 0)
        {
            availableModels.Add(modelName);
        }
        
        selectedModel = availableModels.Contains(modelName) ? modelName : availableModels.First();
    }

    private async Task OnModelChanged()
    {
        if (_agentService != null && !string.IsNullOrEmpty(selectedModel))
        {
            await _agentService.ChangeModel(selectedModel);
            StateHasChanged();
        }
    }

    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(userInput) || _agentService == null)
            return;

        var userMessage = userInput.Trim();
        userInput = string.Empty;

        messages.Add(new ChatMessage
        {
            Role = "User",
            Content = userMessage,
            Timestamp = DateTime.Now
        });

        var assistantMessage = new ChatMessage
        {
            Role = "Assistant",
            Content = "",
            Timestamp = DateTime.Now
        };
        messages.Add(assistantMessage);

        isLoading = true;
        StateHasChanged();

        try
        {
            await _agentService.SendMessageStreamingAsync(userMessage, (token) =>
            {
                InvokeAsync(() =>
                {
                    assistantMessage.Content += token;
                    StateHasChanged();
                });
            });
        }
        catch (Exception ex)
        {
            assistantMessage.Content = $"Errore: {ex.Message}";
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private async Task ClearChat()
    {
        messages.Clear();
        if (_agentService != null)
        {
            await _agentService.ClearHistory();
        }
        StateHasChanged();
    }

    private void ToggleTheme()
    {
        isDarkTheme = !isDarkTheme;
        StateHasChanged();
    }

    private async Task HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !isLoading)
        {
            await SendMessage();
        }
    }
}
