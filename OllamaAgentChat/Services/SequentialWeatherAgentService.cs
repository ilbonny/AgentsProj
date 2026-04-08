using Google.Protobuf;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OllamaAgentChat.Agents;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Linq;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using ChatMessage = OllamaAgentChat.Models.ChatMessage;

namespace OllamaAgentChat.Services;

/// <summary>
/// Servizio con workflow sequenziale di agenti meteo specializzati
/// Processo: Analisi → Allerte → Raccomandazioni → Statistiche
/// Gli agenti vengono eseguiti in sequenza e i risultati vengono combinati
/// </summary>
public class SequentialWeatherAgentService : IAgentService
{
    private List<AIAgent> _agents = null!;
    private readonly List<AgentSession> _sessions = new();
    private string _modelName;
    private readonly string _endpoint;
    private readonly List<ChatMessage> _chatHistory = new();

    public string DeploymentName => $"{_modelName} (Sequential Workflow)";
    public string CurrentModel => _modelName;

    private string _systemInstructions = @"Sei un coordinatore di agenti meteo specializzati.
Quando ricevi una richiesta meteo, coordini una serie di agenti in sequenza per fornire una risposta completa:
1. WeatherAgent - Ottieni dati meteo base
2. WeatherAnalyzerAgent - Analizza le condizioni
3. WeatherAlertAgent - Verifica allerte
4. WeatherAdvisorAgent - Fornisci raccomandazioni
5. WeatherStatisticsAgent - Aggiungi contesto storico

Presenta le informazioni in modo chiaro e strutturato, senza aggiungere commenti ulteriori.";

    private Workflow _workflow;

    public SequentialWeatherAgentService(string endpoint, string modelName)
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

        var chatClient = client.GetChatClient(modelName);

        AIAgent weatherAgent = chatClient.AsAIAgent(
            instructions: "Fornisci informazioni meteo di base: temperatura, condizioni attuali e previsioni.",
            name: "WeatherAgent",
            tools:
            [
                AIFunctionFactory.Create(WeatherAgent.GetWeather),
                AIFunctionFactory.Create(WeatherAgent.GetWeatherForecast),
                AIFunctionFactory.Create(WeatherAgent.GetTemperature)
            ]
        );

        AIAgent analyzerAgent = chatClient.AsAIAgent(
            instructions: "Analizza in dettaglio le condizioni meteorologiche: umidità, vento, pressione, UV.",
            name: "WeatherAnalyzerAgent",
            tools:
            [
                AIFunctionFactory.Create(WeatherAnalyzerAgent.AnalyzeWeatherConditions),
                AIFunctionFactory.Create(WeatherAnalyzerAgent.GetUVIndex)
            ]
        );

        AIAgent alertAgent = chatClient.AsAIAgent(
            instructions: "Verifica allerte meteo e traccia eventuali tempeste o condizioni severe.",
            name: "WeatherAlertAgent",
            tools:
            [
                AIFunctionFactory.Create(WeatherAlertAgent.CheckWeatherAlerts),
                AIFunctionFactory.Create(WeatherAlertAgent.TrackStorms)
            ]
        );

        AIAgent advisorAgent = chatClient.AsAIAgent(
            instructions: "Fornisci raccomandazioni pratiche per attività e abbigliamento basate sul meteo.",
            name: "WeatherAdvisorAgent",
            tools:
            [
                AIFunctionFactory.Create(WeatherAdvisorAgent.GetActivityRecommendations),
                AIFunctionFactory.Create(WeatherAdvisorAgent.GetClothingAdvice)
            ]
        );

        AIAgent statisticsAgent = chatClient.AsAIAgent(
            instructions: "Fornisci dati storici e confronti con medie stagionali.",
            name: "WeatherStatisticsAgent",
            tools:
            [
                AIFunctionFactory.Create(WeatherStatisticsAgent.GetHistoricalWeather),
                AIFunctionFactory.Create(WeatherStatisticsAgent.CompareWithSeasonalAverage)
            ]
        );

        _workflow = new WorkflowBuilder(weatherAgent)
            .AddEdge(weatherAgent, analyzerAgent)
            .AddEdge(analyzerAgent, alertAgent)
            .AddEdge(alertAgent, advisorAgent)
            .AddEdge(advisorAgent, statisticsAgent)
            .Build();

    }

    public async Task ChangeModel(string newModelName)
    {
        _modelName = newModelName;
        await InitializeAgent(newModelName);
    }

    public Task<string> SendMessageAsync(string userMessage)
    {
        throw new NotImplementedException();
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

    public async Task SendMessageStreamingAsync(string userMessage, Action<string> onTokenReceived)
    {
        AddUserMessage(userMessage);

        var fullResponse = new StringBuilder();
        string currentAgentId = string.Empty;
        var currentAgentResponse = new StringBuilder();
        bool hasShownAgentHeader = false;

        try
        {
            await using var streamingRun = await InProcessExecution.RunStreamingAsync(_workflow, userMessage);

            await streamingRun.TrySendMessageAsync(new TurnToken(emitEvents: true));

            await foreach (WorkflowEvent evt in streamingRun.WatchStreamAsync().ConfigureAwait(false))
            {
                // Gestisci AgentResponseUpdate che contiene i risultati delle funzioni
                if (evt.Data is AgentResponseUpdate responseUpdate)
                {
                    var agentId = "Agent";
                    if (evt is AgentResponseUpdateEvent updateEvt)
                    {
                        agentId = updateEvt.ExecutorId ?? "Agent";
                    }

                    // Se cambia l'agente, aggiungi uno spazio separatore
                    if (currentAgentId != agentId)
                    {
                        // Aggiungi separatore se non è il primo agente
                        if (!string.IsNullOrEmpty(currentAgentId))
                        {
                            var separator = "\n\n"; // Due righe vuote per separare gli agenti
                            fullResponse.Append(separator);
                            onTokenReceived(separator);
                        }

                        currentAgentId = agentId;
                        hasShownAgentHeader = false;
                        currentAgentResponse.Clear();
                    }

                    // Mostra l'header una sola volta per agente
                    if (!hasShownAgentHeader)
                    {
                        var agentHeader = $"--- {GetFriendlyAgentName(agentId)} ---\n";
                        fullResponse.Append(agentHeader);
                        onTokenReceived(agentHeader);
                        hasShownAgentHeader = true;
                    }

                    // Estrai e accumula il contenuto
                    var text = ExtractTextFromResponseUpdate(responseUpdate, isStreaming: true);

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        currentAgentResponse.Append(text);
                        fullResponse.Append(text);
                        onTokenReceived(text);
                    }
                }

                // Gestisci AgentResponseEvent (risposta finale completa dell'agente)
                if (evt is AgentResponseEvent agentResponse)
                {
                    var agentId = agentResponse.ExecutorId ?? "Agent";

                    // Se questo evento arriva senza streaming precedente
                    if (currentAgentId != agentId)
                    {
                        // Aggiungi separatore se non è il primo agente
                        if (!string.IsNullOrEmpty(currentAgentId))
                        {
                            var separator = "\n\n";
                            fullResponse.Append(separator);
                            onTokenReceived(separator);
                        }

                        var agentHeader = $"--- {GetFriendlyAgentName(agentId)} ---\n";
                        fullResponse.Append(agentHeader);
                        onTokenReceived(agentHeader);

                        currentAgentId = agentId;
                    }

                    if (!hasShownAgentHeader)
                    {
                        var responseText = agentResponse.Response?.Text ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(responseText))
                        {
                            fullResponse.Append(responseText);
                            onTokenReceived(responseText);
                        }
                    }
                }

                // Gestisci WorkflowOutputEvent (output finale del workflow)
                if (evt is WorkflowOutputEvent outputEvent)
                {
                    if (outputEvent.Data is IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages)
                    {
                        var outputText = string.Join("\n", messages.Select(m => m.Text).Where(t => !string.IsNullOrWhiteSpace(t)));
                        if (!string.IsNullOrWhiteSpace(outputText))
                        {
                            var finalOutput = $"\n\n--- 📊 Risultato Finale ---\n{outputText}\n";
                            fullResponse.Append(finalOutput);
                            onTokenReceived(finalOutput);
                        }
                    }
                }
            }

            AddAssistantMessage(fullResponse.ToString());
        }
        catch (Exception ex)
        {
            var errorMessage = $"Errore nel workflow sequenziale: {ex.Message}";
            AddAssistantMessage(errorMessage);
            onTokenReceived(errorMessage);
        }
    }

    private static ChatClientAgent GetTranslationAgent(string targetLanguage, IChatClient chatClient) =>
        new(chatClient, $"You are a translation assistant that translates the provided text to {targetLanguage}.");

    private static string GetFriendlyAgentName(string agentId)
    {
        // Rimuovi l'ID generato e mostra solo il nome leggibile
        if (agentId.StartsWith("WeatherAgent"))
            return "☀️ Agente Meteo";
        if (agentId.StartsWith("WeatherAnalyzerAgent"))
            return "🔍 Analizzatore Meteo";
        if (agentId.StartsWith("WeatherAlertAgent"))
            return "⚠️ Allerte Meteo";
        if (agentId.StartsWith("WeatherAdvisorAgent"))
            return "💡 Consigli Meteo";
        if (agentId.StartsWith("WeatherStatisticsAgent"))
            return "📊 Statistiche Meteo";

        return agentId;
    }

    private static string ExtractTextFromResponseUpdate(AgentResponseUpdate responseUpdate, bool isStreaming = false)
    {
        var textBuilder = new StringBuilder();

        // Durante lo streaming, mostra solo il testo generato
        if (isStreaming && !string.IsNullOrWhiteSpace(responseUpdate.Text))
        {
            return responseUpdate.Text;
        }

        // Modalità non-streaming: mostra tutto con dettagli
        if (!string.IsNullOrWhiteSpace(responseUpdate.Text))
        {
            textBuilder.AppendLine(responseUpdate.Text);
        }

        // Estrai i contenuti dal messaggio
        if (responseUpdate.Contents != null)
        {
            foreach (var content in responseUpdate.Contents)
            {
                // Gestisci chiamate a funzioni
                if (content is FunctionCallContent functionCall)
                {
                    if (!isStreaming)
                        textBuilder.AppendLine($"🔧 Chiamata: {functionCall.Name}");
                }
                // Gestisci risultati di funzioni
                else if (content is FunctionResultContent functionResult)
                {
                    var result = functionResult.Result?.ToString() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        textBuilder.AppendLine($"✅ {result}");
                    }
                }
                // Gestisci contenuto testuale (solo se non già gestito sopra)
                else if (content is TextContent textContent && string.IsNullOrWhiteSpace(responseUpdate.Text))
                {
                    if (!string.IsNullOrWhiteSpace(textContent.Text))
                    {
                        textBuilder.Append(textContent.Text);
                    }
                }
            }
        }

        return textBuilder.ToString();
    }

    public List<ChatMessage> GetChatHistory()
    {
        return _chatHistory.ToList();
    }

    public async Task ClearHistory()
    {
        _chatHistory.Clear();
    }
}
