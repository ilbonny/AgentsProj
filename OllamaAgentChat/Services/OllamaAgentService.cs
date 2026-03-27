#pragma warning disable SKEXP0010 // Sopprime warning per API sperimentale OpenAI

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OllamaAgentChat.Models;

namespace OllamaAgentChat.Services;

public class OllamaAgentService
{
    private Kernel _kernel = null!;
    private IChatCompletionService _chatService = null!;
    private readonly ChatHistory _chatHistory;
    private readonly string _ollamaEndpoint;
    private string _currentModel;

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

    public void ChangeModel(string newModelName)
    {
        _currentModel = newModelName;
        InitializeKernel(newModelName);
    }

    public async Task<string> SendMessageAsync(string userMessage)
    {
        // Aggiunge il messaggio dell'utente alla cronologia
        _chatHistory.AddUserMessage(userMessage);

        // Ottiene la risposta dall'agent
        var response = await _chatService.GetChatMessageContentAsync(_chatHistory);
        
        // Aggiunge la risposta alla cronologia
        _chatHistory.AddAssistantMessage(response.Content ?? string.Empty);

        return response.Content ?? "Spiacente, non ho ricevuto una risposta.";
    }

    public async Task SendMessageStreamingAsync(string userMessage, Action<string> onTokenReceived)
    {
        // Aggiunge il messaggio dell'utente alla cronologia
        _chatHistory.AddUserMessage(userMessage);

        var fullResponse = string.Empty;

        // Ottiene la risposta in streaming dall'agent
        await foreach (var chunk in _chatService.GetStreamingChatMessageContentsAsync(_chatHistory))
        {
            var content = chunk.Content ?? string.Empty;
            fullResponse += content;
            
            // Notifica ogni token ricevuto
            onTokenReceived(content);
        }

        // Aggiunge la risposta completa alla cronologia
        _chatHistory.AddAssistantMessage(fullResponse);
    }

    public List<ChatMessage> GetChatHistory()
    {
        var messages = new List<ChatMessage>();
        
        foreach (var message in _chatHistory)
        {
            // Salta i messaggi di sistema
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

    public void ClearHistory()
    {
        _chatHistory.Clear();
        _chatHistory.AddSystemMessage(
            "Sei un assistente AI intelligente e cordiale. " +
            "Rispondi in modo preciso e utile alle domande degli utenti."
        );
    }
}
