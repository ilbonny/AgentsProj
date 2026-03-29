using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaAgentChat.Agents;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using ChatMessage = OllamaAgentChat.Models.ChatMessage;

namespace OllamaAgentChat.Services;

public class AzureAgentService : IAgentService
{
    private AIAgent _agent = null!;
    private AgentSession _session = null!;
    private string _modelName;
    private readonly string _endpoint;
    private readonly List<ChatMessage> _chatHistory = new();

    public string DeploymentName => _modelName;
    public string CurrentModel => _modelName;

    private string _systemInstructions = @"Sei un assistente AI intelligente e cordiale. 
Quando chiami una funzione per ottenere informazioni, usa ESATTAMENTE il risultato restituito dalla funzione nella tua risposta.
NON inventare o simulare dati: usa solo i dati reali restituiti dalle funzioni.
Se una funzione restituisce un risultato, presentalo all'utente in modo chiaro senza aggiungere disclaimer o note sulla simulazione.";


    public AzureAgentService(string endpoint, string modelName)
    {
        _endpoint = endpoint;
        _modelName = modelName;

        InitializeAgent(modelName);
    }

    private async Task InitializeAgent(string modelName)
    {
        var client = new OpenAIClient(new ApiKeyCredential("ollama"), new OpenAIClientOptions
        {
            Endpoint = new Uri($"{_endpoint}v1")
        });

         _agent = client.GetChatClient(modelName)
            .AsAIAgent(
                instructions: _systemInstructions,
                name: "OllamaAgent",
                tools: [
                    AIFunctionFactory.Create(WeatherAgent.GetWeather),
                    AIFunctionFactory.Create(WeatherAgent.GetWeatherForecast),
                    AIFunctionFactory.Create(WeatherAgent.GetTemperature)
                ]
        );

        _session = await _agent.CreateSessionAsync();
    }

    public async Task ChangeModel(string newModelName)    
    {
        _modelName = newModelName;  
        await InitializeAgent(newModelName);
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
            // Usa la sessione per mantenere il contesto
            var response = await _agent.RunAsync(userMessage, _session);
            var assistantMessage = response.Text ?? "Spiacente, non ho ricevuto una risposta.";

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
            var stream = _agent.RunStreamingAsync(userMessage, _session);

            await foreach (var update in stream)
            {
                var content = update.Text;
                if (string.IsNullOrEmpty(content)) continue;

                fullResponse += content;
                onTokenReceived(content);
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

    public async Task ClearHistory()
    {
        _chatHistory.Clear();
        _session = await _agent.CreateSessionAsync();
    }
}
