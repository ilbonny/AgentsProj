# Ollama Agent Chat - Blazor Web Chat

Applicazione web Blazor Server minimalista che implementa una chat full-screen con Microsoft Agent Framework e Ollama.

## Caratteristiche

- 🤖 Integrazione con Microsoft Semantic Kernel e Agent Framework
- 💬 Interfaccia chat full-screen moderna e reattiva
- 🦙 Supporto per modelli Ollama (LLaMA, Mistral, ecc.)
- � Cambio modello in tempo reale tramite dropdown
- 📝 Cronologia conversazione
- ⚙️ Configurazione flessibile
- 🎯 UI minimalista focalizzata sulla chat
- ⌨️ Invio messaggi con tasto Enter

## Prerequisiti

1. **.NET 8 SDK** o superiore
2. **Ollama** installato e in esecuzione
   - Download: https://ollama.ai/download
   - Dopo l'installazione, avviare Ollama

## Installazione del Modello Ollama

Prima di utilizzare l'applicazione, scaricare un modello Ollama:

```bash
# Modello consigliato: llama3.2 (piccolo e veloce)
ollama pull llama3.2

# Altri modelli disponibili:
# ollama pull llama3.1
# ollama pull mistral
# ollama pull codellama
```

Per verificare che Ollama sia in esecuzione:
```bash
ollama list
```

## Configurazione

Modificare `appsettings.json` per configurare Ollama:

```json
{
  "Ollama": {
    "Endpoint": "http://localhost:11434/v1",
    "ModelName": "llama3.2"
  }
}
```

### Parametri di configurazione:

- **Endpoint**: URL dell'API di Ollama (default: http://localhost:11434/v1)
- **ModelName**: Nome del modello da utilizzare (es: llama3.2, mistral, codellama)

## Esecuzione

1. Assicurarsi che Ollama sia in esecuzione
2. Avviare l'applicazione:

```bash
cd OllamaAgentChat
dotnet run
```

3. Aprire il browser all'indirizzo mostrato (di solito https://localhost:5001)
4. Cliccare su "Chat AI" nel menu di navigazione

## Debug in Visual Studio Code

Il progetto include configurazioni predefinite per il debug:

### Avviare con il Debugger

1. Aprire la cartella `OllamaAgentChat` in VS Code
2. Premere **F5** oppure andare su **Run > Start Debugging**
3. Il browser si aprirà automaticamente sulla pagina `/chat`

### Break Points

Per impostare un breakpoint:
1. Aprire il file desiderato (es: `Services/OllamaAgentService.cs`)
2. Cliccare sul margine sinistro della riga dove vuoi fermare l'esecuzione
3. Avviare il debug (F5)

### Hot Reload

Per modifiche rapide senza riavviare:
```bash
dotnet watch run
```

### Estensioni VS Code Consigliate

Il progetto suggerirà automaticamente queste estensioni:
- **C# Dev Kit** - Supporto completo per C#
- **C#** - IntelliSense e debugging
- **.NET Runtime** - Runtime .NET

## Architettura

### Componenti Principali

- **OllamaAgentService**: Servizio che gestisce la comunicazione con Ollama tramite Semantic Kernel
- **Chat.razor**: Componente Blazor per l'interfaccia utente della chat
- **ChatMessage**: Modello per i messaggi della conversazione

### Tecnologie Utilizzate

- **Blazor Server** (.NET 8)
- **Microsoft Semantic Kernel**: Framework per AI agents
- **Ollama**: Runtime per modelli LLM locali

## Personalizzazione

### Modificare il comportamento dell'Agent

Modificare il messaggio di sistema in `Services/OllamaAgentService.cs`:

```csharp
_chatHistory.AddSystemMessage(
    "Il tuo messaggio di sistema personalizzato qui..."
);
```

### Aggiungere funzionalità

È possibile estendere l'agent con:
- **Plugins**: Aggiungere funzioni che l'agent può chiamare
- **Memory**: Implementare memoria persistente
- **Planners**: Aggiungere capacità di planning automatico

## Risoluzione Problemi

### Ollama non si connette

- Verificare che Ollama sia in esecuzione: `ollama list`
- Controllare che l'endpoint in `appsettings.json` sia corretto
- Su Windows, Ollama usa di default la porta 11434

### Il modello non risponde

- Verificare che il modello sia scaricato: `ollama list`
- Controllare che il nome del modello in `appsettings.json` corrisponda esattamente

### Errori di compilazione

Eseguire:
```bash
dotnet restore
dotnet build
```

## Prossimi Sviluppi

- [ ] Supporto per streaming delle risposte
- [ ] Salvataggio conversazioni
- [ ] Supporto per upload di file
- [ ] Integrazione con altri provider LLM (Azure OpenAI, OpenAI)
- [ ] Funzioni e tools personalizzati

## Licenza

Questo progetto è fornito "as-is" a scopo educativo e dimostrativo.
