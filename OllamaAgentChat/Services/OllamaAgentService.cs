#pragma warning disable SKEXP0010 // Sopprime warning per API sperimentale OpenAI

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaAgentChat.Models;

namespace OllamaAgentChat.Services;

public class OllamaAgentService: IAgentService
{
    private Kernel _kernel = null!;
    private IChatCompletionService _chatService = null!;
    private readonly ChatHistory _chatHistory;
    private readonly string _ollamaEndpoint;
    private string _currentModel;

    public string DeploymentName { get; }
    public string CurrentModel => _currentModel;

    public OllamaAgentService(string ollamaEndpoint, string modelName)
    {
        _ollamaEndpoint = ollamaEndpoint;
        _currentModel = modelName;
        _chatHistory = new ChatHistory();
        
        InitializeKernel(modelName);
        
        _chatHistory.AddSystemMessage(
            "Sei un assistente AI intelligente e cordiale. " +
            "Rispondi in modo preciso e utile alle domande degli utenti."
        );
    }

    private void InitializeKernel(string modelName)
    {
        var builder = Kernel.CreateBuilder();
        
        builder.AddOpenAIChatCompletion(
            modelId: modelName,
            apiKey: "ollama",
            endpoint: new Uri(_ollamaEndpoint)
        );

        _kernel = builder.Build();
        _chatService = _kernel.GetRequiredService<IChatCompletionService>();
    }

    public Task ChangeModel(string newModelName)
    {
        _currentModel = newModelName;
        InitializeKernel(newModelName);

        return Task.CompletedTask;
    }

    public async Task<string> SendMessageAsync(string userMessage)
    {
        _chatHistory.AddUserMessage(userMessage);

        var response = await _chatService.GetChatMessageContentAsync(_chatHistory);
        
        _chatHistory.AddAssistantMessage(response.Content ?? string.Empty);

        return response.Content ?? "Spiacente, non ho ricevuto una risposta.";
    }

    public async Task SendMessageStreamingAsync(string userMessage, Action<string> onTokenReceived)
    {
        _chatHistory.AddUserMessage(userMessage);

        var fullResponse = string.Empty;

        await foreach (var chunk in _chatService.GetStreamingChatMessageContentsAsync(_chatHistory))
        {
            var content = chunk.Content ?? string.Empty;
            fullResponse += content;
            
            onTokenReceived(content);
        }

        _chatHistory.AddAssistantMessage(fullResponse);
    }

    public List<ChatMessage> GetChatHistory()
    {
        var messages = new List<ChatMessage>();
        
        foreach (var message in _chatHistory)
        {
            if (message.Role.ToString() == "System")
                continue;

            messages.Add(new ChatMessage
            {
                Role = message.Role.ToString(),
                Content = message.Content ?? string.Empty,
                Timestamp = DateTime.Now
            });
        }

        return messages;
    }

    public Task ClearHistory()
    {
        _chatHistory.Clear();
        _chatHistory.AddSystemMessage(
            "Sei un assistente AI intelligente e cordiale. " +
            "Rispondi in modo preciso e utile alle domande degli utenti."
        );

        return Task.CompletedTask;
    }
}
