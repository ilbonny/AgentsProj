using OllamaAgentChat.Models;

namespace OllamaAgentChat.Services;

public interface IAgentService
{
    string DeploymentName { get; }
    string CurrentModel { get; }
    Task ChangeModel(string newModelName);
    Task<string> SendMessageAsync(string userMessage);
    Task SendMessageStreamingAsync(string userMessage, Action<string> onTokenReceived);
    List<ChatMessage> GetChatHistory();
    Task ClearHistory();
}