# Ollama Agent Chat - Blazor Web Chat

Applicazione web Blazor Server minimalista che implementa una chat full-screen con Ollama utilizzando OpenAI SDK.

## Caratteristiche

- 🤖 Integrazione con Ollama tramite OpenAI SDK
- 💬 Interfaccia chat full-screen moderna e reattiva
- 🦙 Supporto per tutti i modelli Ollama (LLaMA, Mistral, ecc.)
- 📝 Cronologia conversazione con streaming in tempo reale
- ⚙️ Configurazione flessibile
- 🎯 UI minimalista focalizzata sulla chat
- ⌨️ Invio messaggi con tasto Enter
- 🌙 Tema scuro

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
ollama pull llama3.1:8b
ollama pull mistral
ollama pull codellama
ollama pull phi3
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
dotnet run
```

3. Aprire il browser all'indirizzo mostrato (di solito https://localhost:5001)
4. La pagina chat si aprirà automaticamente

## Debug in Visual Studio Code

Il progetto include configurazioni predefinite per il debug:

### Avviare con il Debugger

1. Aprire la cartella del progetto in VS Code
2. Premere **F5** oppure andare su **Run > Start Debugging**
3. Il browser si aprirà automaticamente sulla pagina `/chat`

### Hot Reload

Per modifiche rapide senza riavviare:
```bash
dotnet watch run
```

### Estensioni VS Code Consigliate

Il progetto suggerirà automaticamente queste estensioni:
- **C# Dev Kit** - Supporto completo per C#
- **C#** - IntelliSense e debugging

## Architettura

### Componenti Principali

- **AzureAgentService**: Servizio che gestisce la comunicazione con Ollama tramite OpenAI SDK
- **Chat.razor**: Componente Blazor per l'interfaccia utente della chat
- **ChatMessage**: Modello per i messaggi della conversazione

### Tecnologie Utilizzate

- **Blazor Server** (.NET 8)
- **Azure.AI.OpenAI**: SDK OpenAI compatibile con Ollama
- **OpenAI**: Libreria client OpenAI
- **System.ClientModel**: Per l'autenticazione API

## Pacchetti NuGet

```xml
<PackageReference Include="Azure.AI.OpenAI" Version="2.1.0" />
<PackageReference Include="Azure.Identity" Version="1.19.0" />
<PackageReference Include="Microsoft.Agents.AI.OpenAI" Version="1.0.0-rc4" />
```

## Personalizzazione

### Modificare il comportamento dell'Agent

Modificare il messaggio di sistema in `Services/AzureAgentService.cs`:

```csharp
private readonly string _systemInstructions = 
    "Il tuo messaggio di sistema personalizzato qui...";
```

### Modelli Ollama Consigliati

- **llama3.2**: Modello piccolo e veloce (3B parametri)
- **llama3.1:8b**: Modello bilanciato (8B parametri)
- **mistral**: Ottimo per uso generico
- **codellama**: Specializzato per codice
- **phi3**: Molto leggero (3.8B parametri)

## Streaming in Tempo Reale

L'applicazione supporta lo streaming delle risposte, mostrando il testo mentre viene generato dal modello.

## Risoluzione Problemi

### Ollama non si connette

```
Error: Connection refused
```

- Verificare che Ollama sia in esecuzione: `ollama list`
- Controllare che l'endpoint in `appsettings.json` sia corretto (http://localhost:11434/v1)
- Su Windows, Ollama usa di default la porta 11434

### Il modello non risponde

- Verificare che il modello sia scaricato: `ollama list`
- Controllare che il nome del modello in `appsettings.json` corrisponda esattamente
- Provare a testare il modello direttamente: `ollama run llama3.2`

### Errori di compilazione

Eseguire:
```bash
dotnet restore
dotnet build
```

### Il modello è lento

- Considerare l'uso di un modello più piccolo (es: phi3, llama3.2)
- Verificare le risorse del sistema (RAM, GPU)
- Assicurarsi che nessun altro processo stia usando Ollama

## Vantaggi di questo approccio

- ✅ **Privacy**: I dati rimangono sul tuo computer
- ✅ **Offline**: Non richiede connessione internet
- ✅ **Gratuito**: Nessun costo API
- ✅ **Veloce**: Risposte istantanee su hardware adeguato
- ✅ **Standard**: Usa l'API OpenAI compatibile

## Prossimi Sviluppi

- [x] Supporto per streaming delle risposte
- [ ] Cambio modello dinamico tramite UI
- [ ] Salvataggio conversazioni
- [ ] Supporto per upload di file
- [ ] Funzioni e tools personalizzati
- [ ] Supporto multimodale (con modelli compatibili)

## Licenza

Questo progetto è fornito "as-is" a scopo educativo e dimostrativo.
