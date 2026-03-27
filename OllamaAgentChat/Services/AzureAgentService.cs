using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using OllamaAgentChat.Models;
using ChatMessage = OllamaAgentChat.Models.ChatMessage;

namespace OllamaAgentChat.Services;

public class AzureAgentService
{
    private readonly dynamic _agent;
    private readonly string _modelName;
    private readonly List<ChatMessage> _chatHistory = new();

    public string DeploymentName => _modelName;

    public AzureAgentService(string endpoint, string modelName)
    {
        _modelName = modelName;
        
        var client = new OpenAIClient(new ApiKeyCredential("ollama"), new OpenAIClientOptions
        {
            Endpoint = new Uri(endpoint + "v1")
        });
        
        _agent = client.GetChatClient(modelName).AsAIAgent(
            instructions: "Sei un assistente AI intelligente e cordiale. Rispondi in modo preciso e utile alle domande degli utenti.",
            name: "OllamaAgent"
        );
    }

    private void AddUserMessage(string content)
    {
        _chatHistory.Add(new ChatMessage
        {
            Role = "User",
            Content = content,
            Timestamp = DateTime.Now
        });
    }

    private void AddAssistantMessage(string content)
    {
        _chatHistory.Add(new ChatMessage
        {
            Role = "Assistant",
            Content = content,
            Timestamp = DateTime.Now
        });
    }

    public async Task<string> SendMessageAsync(string userMessage)
    {
        AddUserMessage(userMessage);

        try
        {
            var response = await _agent.InvokeAsync(userMessage);
            var assistantMessage = response.Content ?? "Spiacente, non ho ricevuto una risposta.";
            
            AddAssistantMessage(assistantMessage);
            return assistantMessage;
        }
        catch (Exception ex)
        {
            var errorMessage = $"Errore: {ex.Message}";
            AddAssistantMessage(errorMessage);
            return errorMessage;
        }
    }

    public async Task SendMessageStreamingAsync(string userMessage, Action<string> onTokenReceived)
    {
        AddUserMessage(userMessage);

        var fullResponse = string.Empty;

        try
        {
            var stream = _agent.RunStreamingAsync(userMessage);
            
            await foreach (var update in (IAsyncEnumerable<dynamic>)stream)
            {
                if (update.Content != null)
                {
                    fullResponse += update.Content;
                    onTokenReceived(update.Content);
                }
            }

            AddAssistantMessage(fullResponse);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Errore: {ex.Message}";
            AddAssistantMessage(errorMessage);
            onTokenReceived(errorMessage);
        }
    }

    public List<ChatMessage> GetChatHistory()
    {
        return _chatHistory.ToList();
    }

    public void ClearHistory()
    {
        _chatHistory.Clear();
    }
}
