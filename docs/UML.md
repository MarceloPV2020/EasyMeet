# UML - EasyMeet

Este documento apresenta diagramas Mermaid para representar a arquitetura atual do EasyMeet.

## Arquitetura Geral

```mermaid
flowchart TB
    UI[Interface Web - wwwroot] --> Controllers[Controllers ASP.NET Core]
    Controllers --> Services[Services]
    Services --> Providers[Providers de IA]
    Services --> Repository[SQLite Repository]
    Services --> Credentials[Windows Credential Manager]
    Providers --> OpenRouter[OpenRouter]
    Providers --> Direct[Providers Diretos]
    Direct --> Gemini[Gemini]
    Direct --> Groq[Groq]
    Direct --> OpenAI[OpenAI]
    Direct --> Anthropic[Anthropic]
    Direct --> Mistral[Mistral]
    Direct --> Cohere[Cohere]
        UI --> PDF[jsPDF]
```

## Fluxo de Análise

```mermaid
sequenceDiagram
    autonumber
    actor Usuário
    participant UI as Interface Web
    participant Controller as ReunioesController
    participant Agent as AgenteResumoReuniaoService
    participant Store as IApiKeyStore
    participant Factory as AIProviderFactory
    participant Provider as IGenerativeAIClient
    participant Parser as MeetingAnalysisParser
    participant Repo as IReuniaoRepository

    Usuário->>UI: Informa transcrição e escolhe provider/modelo
    UI->>Controller: POST /api/reunioes/analisar
    Controller->>Controller: Valida texto
    Controller->>Agent: ResumirAsync(...)
    Agent->>Store: Busca API key do provider
    Agent->>Factory: Resolve provider
    Agent->>Provider: Envia prompt e configuração
    Provider-->>Agent: Retorna conteúdo gerado pelo LLM
    Agent->>Parser: Valida e normaliza JSON
    Agent-->>Controller: Retorna resposta estruturada
    Controller->>Repo: Salva histórico
    Controller-->>UI: Retorna análise
```

## Providers de IA

```mermaid
classDiagram
    class IGenerativeAIClient {
        <<interface>>
        +ProvedorIA Provedor
        +GerarConteudoAsync(prompt, apiKey, configuracaoIA, cancellationToken)
        +TestarConexaoAsync(apiKey, cancellationToken)
    }

    class OpenRouterClientService
    class GeminiClientService
    class GroqClientService
    class OpenAIClientService
    class AnthropicClientService
    class MistralClientService
    class CohereClientService

    IGenerativeAIClient <|.. OpenRouterClientService
    IGenerativeAIClient <|.. GeminiClientService
    IGenerativeAIClient <|.. GroqClientService
    IGenerativeAIClient <|.. OpenAIClientService
    IGenerativeAIClient <|.. AnthropicClientService
    IGenerativeAIClient <|.. MistralClientService
    IGenerativeAIClient <|.. CohereClientService
```

## Controllers e Services

```mermaid
classDiagram
    class ReunioesController {
        +ListarAsync(cancellationToken)
        +ResumirAsync(request, cancellationToken)
        +RemoverAsync(id, cancellationToken)
    }

    class IAConfigController {
        +ListarProvedores()
        +ObterStatus(cancellationToken)
        +SalvarCredencial(request, cancellationToken)
        +RemoverCredencial(provedor, cancellationToken)
        +TestarCredencial(request, cancellationToken)
        +VerificarModelosOpenRouterAsync(request, cancellationToken)
    }

    class IAgenteResumoReuniaoService {
        <<interface>>
        +ResumirAsync(transcricao, provedorIA, configuracaoIA, cancellationToken)
    }

    class AgenteResumoReuniaoService
    class AIProviderFactory
    class MeetingAnalysisParser
    class SqliteReuniaoRepository

    ReunioesController --> IAgenteResumoReuniaoService
    ReunioesController --> SqliteReuniaoRepository
    AgenteResumoReuniaoService ..|> IAgenteResumoReuniaoService
    AgenteResumoReuniaoService --> AIProviderFactory
    AgenteResumoReuniaoService --> MeetingAnalysisParser
```

## Relacionamento Das Entidades

```mermaid
classDiagram
    class ResumoReuniaoRequest {
        +string Transcricao
        +ProvedorIA ProvedorIA
        +ConfiguracaoAnaliseIA ConfiguracaoIA
    }

    class ConfiguracaoAnaliseIA {
        +string Modelo
        +double Temperatura
        +int MaxTokens
    }

    class ResumoReuniaoResponse {
        +string Resumo
        +string[] TopicosPrincipais
        +AcaoReuniaoItem[] Acoes
        +string[] Responsaveis
        +string[] Decisoes
        +string[] Pendencias
        +string DataReuniao
        +string TipoReuniao
        +double NivelConfianca
        +bool GeradoPorIA
        +string ModoExecucao
        +string ModeloIA
    }

    class Reuniao {
        +int Id
        +string Transcricao
        +string Resumo
        +string TipoReuniao
        +decimal Confianca
        +bool GeradoPorIA
        +string ModoExecucao
        +string ModeloIA
    }

    class AcaoReuniaoItem {
        +string Descricao
        +string Responsavel
        +string Prazo
    }

    ResumoReuniaoRequest --> ConfiguracaoAnaliseIA
    ResumoReuniaoResponse --> AcaoReuniaoItem
    Reuniao --> AcaoReuniaoItem
```

## Histórico e PDF

```mermaid
flowchart LR
    Analysis[Análise concluída] --> Save[Salvar no SQLite]
    Save --> History[Histórico na UI]
    History --> Filters[Filtros por texto, data e tipo]
    History --> Remove[Remover registro]
    History --> Export[Gerar PDF]
    Export --> PDF[Relatório PDF]
```

## Fluxo Multi-Provider

```mermaid
flowchart TD
    Request[Request com ProvedorIA] --> Factory[AIProviderFactory]
    Factory --> Choice{Provider selecionado}
    Choice -->|OpenRouter| OR[OpenRouterClientService]
    Choice -->|Gemini| GE[GeminiClientService]
    Choice -->|Groq| GR[GroqClientService]
    Choice -->|OpenAI| OA[OpenAIClientService]
    Choice -->|Anthropic| AN[AnthropicClientService]
    Choice -->|Mistral| MI[MistralClientService]
    Choice -->|Cohere| CO[CohereClientService]
```
