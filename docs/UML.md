# Documentação de Arquitetura e Diagramas UML - EasyMeet

## 1. Diagrama de Classes (Estrutura da API)

```mermaid
classDiagram
    class ReunioesController {
        +ResumirAsync(request, cancellationToken)
    }

    class IAConfigController {
        +ListarProvedores()
        +ObterStatus(cancellationToken)
        +SalvarCredencial(request, cancellationToken)
        +RemoverCredencial(provedor, cancellationToken)
        +TestarCredencial(request, cancellationToken)
    }

    class IAgenteResumoReuniaoService {
        <<interface>>
        +ResumirAsync(transcricao, provedorIA, configuracaoIA, cancellationToken)
    }

    class AgenteResumoReuniaoService {
        -PromptResumoReuniaoBuilder promptBuilder
        -IApiKeyStore apiKeyStore
    }

    ReunioesController --> IAgenteResumoReuniaoService
    AgenteResumoReuniaoService ..|> IAgenteResumoReuniaoService
    sequenceDiagram
    autonumber
    Actor Usuário
    Participant API as EasyMeet.Api (ReunioesController)
    Participant Service as AgenteResumoReuniaoService
    Participant LLM as Provedor de IA (Gemini/OpenAI/Groq)

    Usuário->>API: POST /api/reunioes/resumir (Transcrição + Provedor)
    API->>Service: ResumirAsync(transcricao, provedor)
    Note over Service: Recupera API Key salva e<br/>monta o prompt estruturado
    Service->>LLM: Requisição HTTP (Prompt + Transcrição)
    LLM-->>Service: Retorna Resumo (JSON/Texto)
    Service-->>API: Entrega o resumo processado
    API-->>Usuário: Retorna Status 200 (Resumo Gerado com Sucesso)