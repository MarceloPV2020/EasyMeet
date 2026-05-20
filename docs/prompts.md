# Prompts Utilizados no Projeto EasyMeet

Este documento registra os prompts usados ao longo do desenvolvimento do EasyMeet, organizados por etapa, com objetivo e resultado esperado.

## 1. Criar estrutura inicial do projeto
### Prompt
Crie toda a estrutura inicial do projeto ASP.NET Core .NET 9 chamado EasyMeet.

Objetivo do sistema:
Criar uma API REST que recebe transcrições ou textos de reuniões e utiliza IA para:
- gerar resumo;
- identificar tópicos principais;
- identificar ações;
- identificar responsáveis;
- classificar o tipo da reunião;
- estruturar automaticamente as informações.

A IA deve desempenhar papel funcional no produto.

Quero:
- comandos PowerShell;
- criação da solution;
- criação da Web API .NET 9;
- criação do projeto xUnit;
- criação das pastas;
- criação dos arquivos iniciais;
- inicialização do git;
- criação da branch feature/easymeet-ia.

Estrutura desejada:

EasyMeet/
├── src/
│   └── EasyMeet.Api
├── tests/
│   └── EasyMeet.Tests
├── docs/
├── .github/
└── README.md

Criar também:
- .gitignore
- pull_request_template.md
- README.md inicial

Os comandos devem funcionar no PowerShell do Windows.

## 2. Criar arquitetura da API
### Prompt
Agora gere toda a arquitetura da API ASP.NET Core .NET 9 para o projeto EasyMeet.Api.

Objetivo:
A API deve receber transcrições de reuniões e utilizar IA para gerar:
- resumo;
- tópicos principais;
- ações;
- responsáveis;
- classificação da reunião;
- nível de confiança da análise.

Requisitos técnicos:
- ASP.NET Core Web API;
- Swagger;
- Controllers;
- Services;
- DTOs;
- injeção de dependência;
- nullable enabled;
- async/await;
- arquitetura organizada;
- Clean Code;
- comentários XML;
- endpoint POST /api/reunioes/resumir;
- tratamento básico de erros;
- validações.

Estrutura obrigatória:

src/EasyMeet.Api/
├── Controllers/
├── Services/
├── Models/
├── Prompts/
└── Program.cs

Crie:
- ReunioesController
- IAgenteResumoReuniaoService
- AgenteResumoReuniaoService
- PromptResumoReuniaoBuilder
- GeminiClientService
- ResumoReuniaoRequest
- ResumoReuniaoResponse
- GeminiSettings

A resposta da API deve conter:

{
  "resumo": "",
  "topicosPrincipais": [],
  "acoes": [],
  "responsaveis": [],
  "tipoReuniao": "",
  "nivelConfianca": 0.0,
  "geradoPorIA": true,
  "modoExecucao": ""
}

Configure Swagger corretamente no Program.cs.

## 3. Criar agente de IA real com Gemini
### Prompt
Implemente um agente de IA real usando Google Gemini API.

Requisitos:
- usar Gemini 2.5 Flash;
- usar variável de ambiente GEMINI_API_KEY;
- criar integração real via HttpClient;
- criar fallback local caso a IA falhe;
- usar prompt engineering;
- separar responsabilidades corretamente.

Criar:
- GeminiClientService
- PromptResumoReuniaoBuilder
- arquivo Prompts/resumo-reuniao-agent-prompt.txt

O agente deve:
- resumir reunião;
- identificar ações;
- identificar responsáveis;
- classificar reunião;
- estruturar dados;
- retornar JSON válido.

Formato obrigatório:

{
  "resumo": "",
  "topicosPrincipais": [],
  "acoes": [],
  "responsaveis": [],
  "tipoReuniao": "",
  "nivelConfianca": 0.0
}

Quando usar IA real:
{
  "geradoPorIA": true,
  "modoExecucao": "gemini"
}

Quando usar fallback:
{
  "geradoPorIA": false,
  "modoExecucao": "fallback_local"
}

Implementar:
- timeout;
- tratamento de exceções;
- logs básicos;
- desserialização segura;
- validação do JSON retornado pela IA.

## 4. Criar endpoint REST
### Prompt
Crie o endpoint POST /api/reunioes/resumir.

Entrada:

{
  "texto": ""
}

Saída:

{
  "resumo": "",
  "topicosPrincipais": [],
  "acoes": [],
  "responsaveis": [],
  "tipoReuniao": "",
  "nivelConfianca": 0.0,
  "geradoPorIA": true,
  "modoExecucao": ""
}

Requisitos:
- validar texto vazio;
- validar texto muito curto;
- retornar BadRequest adequadamente;
- usar CancellationToken;
- manter Swagger funcionando;
- manter arquitetura limpa.

## 5. Criar testes automatizados
### Prompt
Crie testes automatizados xUnit para o projeto EasyMeet.Tests.

Criar no mínimo 5 testes automatizados cobrindo:

1. Texto vazio.
2. Texto muito curto.
3. Resumo gerado corretamente.
4. Identificação de ações.
5. Identificação de responsáveis.
6. Identificação de tópicos principais.
7. Funcionamento do fallback local.
8. Funcionamento da integração Gemini.

Requisitos:
- padrão AAA;
- async/await;
- evitar chamadas reais para Gemini nos testes;
- usar mocks/fakes;
- garantir funcionamento do dotnet test.

## 6. Criar README.md
### Prompt
Crie um README.md profissional para o projeto EasyMeet.

O README deve conter:
- descrição do projeto;
- problema resolvido;
- papel da IA;
- tecnologias utilizadas;
- arquitetura;
- estrutura de pastas;
- fluxo da aplicação;
- como executar;
- como configurar GEMINI_API_KEY;
- como rodar testes;
- exemplo de request;
- exemplo de response;
- fluxograma Mermaid;
- decisões técnicas;
- limitações conhecidas;
- melhorias futuras.

O README deve ser adequado para uma atividade acadêmica de IA para desenvolvimento de software.

## 7. Criar PRD.md
### Prompt
Crie o arquivo docs/PRD.md.

O documento deve conter:
- visão geral;
- objetivo do produto;
- problema;
- público-alvo;
- solução proposta;
- funcionalidades;
- papel da IA;
- requisitos funcionais;
- requisitos não funcionais;
- regras de negócio;
- limitações;
- roadmap futuro.

Formato markdown.

## 8. Criar ADR.md
### Prompt
Crie o arquivo docs/ADR.md.

Documente as principais decisões arquiteturais do projeto.

Inclua:
- problema técnico;
- alternativas consideradas;
- decisão tomada;
- justificativa;
- consequências da decisão.

Documente pelo menos:
- uso do Gemini;
- uso de fallback local;
- uso de ASP.NET Core .NET 9;
- separação em Services/Controllers/Models;
- uso de prompt engineering;
- uso de testes automatizados.

## 9. Criar DIRETRIZES_IA.md
### Prompt
Crie o arquivo docs/DIRETRIZES_IA.md.

O documento deve definir:
- papel da IA no sistema;
- comportamento esperado do agente;
- regras de prompt engineering;
- formato obrigatório das respostas;
- limitações da IA;
- regras de fallback;
- validações;
- segurança;
- prevenção de respostas inválidas;
- tratamento de falhas;
- regras para respostas JSON.

Explique como a IA deve:
- resumir reuniões;
- identificar ações;
- identificar responsáveis;
- classificar reuniões;
- evitar inventar informações.

## 10. Criar prompts.md
### Prompt
Crie o arquivo docs/prompts.md.

Documente todos os prompts utilizados no desenvolvimento do projeto.

Organize por etapa:
- criação da arquitetura;
- criação da API;
- criação do agente de IA;
- integração Gemini;
- criação dos testes;
- criação do README;
- criação do PRD;
- criação do backlog;
- criação do UML;
- criação do ADR;
- criação das diretrizes de IA;
- refatorações.

## 11. Refatoração e Mlehorias com novos Provedores
### Prompt
Refatore o projeto EasyMeet para suportar múltiplos provedores de IA configuráveis e selecionáveis pelo usuário em tempo de execução.

Objetivo:
Transformar o EasyMeet em uma plataforma profissional de análise inteligente de reuniões com suporte a múltiplas IA, gerenciamento local seguro de credenciais e seleção dinâmica do provedor durante o uso.

A aplicação é local Windows.

Provedores iniciais:
- Gemini
- Groq

Arquitetura desejada:

EasyMeet
├── Providers
│   ├── Gemini
│   ├── Groq
│   └── Factory
│
├── Credentials
│   ├── Save API Key
│   ├── Load API Key
│   ├── Update API Key
│   ├── Delete API Key
│   └── Windows Credential Manager
│
├── Meeting Analysis
│   ├── Select Provider
│   ├── Generate Summary
│   ├── Extract Actions
│   ├── Extract Decisions
│   └── Extract Responsibilities

Requisitos:

1. Criar enum:
   ProvedorIA

Valores:
- Gemini
- Groq

2. Criar interface:
   IGenerativeAIClient

Métodos:
- ProvedorIA Provedor { get; }
- Task<string> GerarConteudoAsync(string prompt, string apiKey, CancellationToken cancellationToken)
- Task<bool> TestarConexaoAsync(string apiKey, CancellationToken cancellationToken)

3. Refatorar:
- GeminiClientService
- GroqClientService

Ambos devem implementar:
- IGenerativeAIClient

4. Criar:
   IAProviderFactory

Responsável por:
- localizar provider correto;
- validar provider;
- retornar provider conforme seleção do usuário.

5. Criar interface:
   IApiKeyStore

Métodos:
- Task SaveApiKeyAsync(ProvedorIA provedor, string apiKey, CancellationToken cancellationToken)
- Task<string?> GetApiKeyAsync(ProvedorIA provedor, CancellationToken cancellationToken)
- Task DeleteApiKeyAsync(ProvedorIA provedor, CancellationToken cancellationToken)
- Task<bool> HasApiKeyAsync(ProvedorIA provedor, CancellationToken cancellationToken)

6. Criar implementação:
   WindowsCredentialApiKeyStore

Usar:
- Windows Credential Manager

Salvar por provedor:
- EasyMeet:Gemini
- EasyMeet:Groq

Nunca salvar em:
- appsettings.json
- launchSettings.json
- txt/json local
- logs
- GitHub

7. Criar:
   ApiKeyManagerService

Responsabilidades:
- salvar chave;
- alterar chave;
- remover chave;
- verificar status;
- testar conexão.

8. Criar controller:
   IAConfigController

Endpoints:
- GET /api/ia/provedores
- GET /api/ia/credenciais/status
- POST /api/ia/credenciais
- DELETE /api/ia/credenciais/{provedor}
- POST /api/ia/credenciais/testar

9. Criar models:
- SalvarApiKeyRequest
- TestarApiKeyRequest
- ApiKeyStatusResponse
- ProvedorIAResponse

10. Alterar ResumoReuniaoRequest:

{
  "transcricao": "",
  "provedorIA": "Gemini"
}

Não receber apiKey nesse endpoint.

11. Alterar AgenteResumoReuniaoService:

Fluxo:
- receber ProvedorIA;
- buscar chave salva no IApiKeyStore;
- validar existência da chave;
- selecionar provider via IAProviderFactory;
- gerar prompt;
- chamar provider;
- retornar resposta estruturada.

12. Caso não exista chave:
retornar erro claro:

"Chave de API não configurada para o provedor selecionado."

13. Atualizar Swagger:
- permitir seleção do enum ProvedorIA;
- documentar providers disponíveis.

14. Atualizar interface web:

Criar duas áreas:

A) Configuração das IA
- select Gemini/Groq
- campo password API Key
- botão Salvar chave
- botão Alterar chave
- botão Remover chave
- botão Testar conexão
- exibir:
  - configurado
  - não configurado

B) Análise da reunião
- select Gemini/Groq
- textarea transcrição
- botão Analisar reunião
- exibir:
  - resumo
  - tópicos
  - ações
  - responsáveis
  - decisões
  - pendências
  - tipo reunião
  - confiança

15. Melhorar UX:
- interface moderna;
- cards;
- loading;
- mensagens de erro amigáveis;
- responsividade;
- aparência profissional.

16. Segurança:
- nunca retornar apiKey;
- nunca logar apiKey;
- mascarar visualmente a chave;
- permitir substituição da chave;
- remover chave permanentemente quando solicitado.

17. Atualizar prompt da IA para:

- resumo corporativo;
- ações estruturadas;
- decisões tomadas;
- pendências;
- responsáveis;
- classificação automática da reunião;
- consolidação de ações duplicadas;
- resposta somente JSON válido.

18. Atualizar testes automatizados:
- salvar chave Gemini;
- salvar chave Groq;
- alterar chave;
- remover chave;
- testar conexão;
- selecionar provider corretamente;
- bloquear análise sem chave;
- validar providers inválidos;
- validar resposta IA;
- garantir:
  dotnet test EasyMeet.sln

19. Atualizar:
- README.md
- docs/PRD.md
- docs/VIABILIDADE.md
- docs/ADR.md
- docs/DIRETRIZES_IA.md
- docs/prompts.md
- PR.md

20. Manter:
- Clean Code
- SOLID
- async/await
- DI
- Swagger
- GitHub Actions
- arquitetura organizada

21. Não implementar fallback local.
Toda análise válida deve utilizar IA real.

22. Toda a aplicação deve permanecer funcional com:
- dotnet build EasyMeet.sln
- dotnet test EasyMeet.sln
- dotnet run --project .\src\EasyMeet.Api\EasyMeet.Api.csproj

## 12. Revisão final da atividade
### Prompt
Revise todo o projeto EasyMeet considerando os requisitos da atividade acadêmica.

Verifique:
- README.md
- PRD.md
- VIABILIDADE.md
- BACKLOG.md
- UML.md
- ADR.md
- DIRETRIZES_IA.md
- prompts.md
- Swagger funcionando
- integração Gemini funcionando
- agente de IA explícito
- fallback local funcionando
- pelo menos 5 testes automatizados
- arquitetura organizada
- Pull Request template criado
- documentação completa
- user stories
- diagramas UML
- interface web funcionando

Corrija automaticamente qualquer problema encontrado.

Depois informe:
- quais requisitos foram atendidos;
- o que ainda falta;
- comandos finais para:
  - dotnet build
  - dotnet test
  - dotnet run

## 13. Submissão ao Repositório Git com Pull Request
### Prompt
Prepare a submissão final do projeto EasyMeet para GitHub.

Objetivo:
Submeter todas as alterações atuais em uma branch feature/easymeet-ia e preparar um Pull Request completo.

Execute as seguintes etapas:

1. Verifique alterações pendentes.
2. Verifique a branch atual.
3. Caso necessário, troque/crie a branch:
   feature/easymeet-ia

4. Adicione todos os arquivos:
   git add .

5. Faça commit utilizando:
   feat: implementa easymeet com assistente inteligente de reunioes

6. Faça push para:
   origin feature/easymeet-ia

7. Verifique se existe:
   .github/workflows/dotnet-ci.yml

8. Verifique se existe:
   PR.md

9. Verifique se os testes executam:
   dotnet build EasyMeet.sln
   dotnet test EasyMeet.sln

10. Gere instruções finais para:
    - abrir Pull Request;
    - validar GitHub Actions;
    - confirmar execução automática dos testes.

11. Utilizar o conteúdo do arquivo PR.md como descrição do Pull Request.

Não faça merge automaticamente.
O Pull Request deve permanecer OPEN.

## 14. Requisito: Cobertura Mínima de Testes - 30%
### Prompt
Atualize as políticas de contribuição para exigir cobertura mínima de testes automatizados de 30% antes de aprovação e merge de Pull Requests.

## Obrigatoriedade
Todo Pull Request deve atender a uma cobertura mínima de **30%** de testes automatizados antes de ser aprovado e mesclado.

## O que é considerado
- Testes unitários (xUnit, NUnit, MSTest, etc.)
- Testes de integração
- Testes que validem o comportamento da funcionalidade
- Linhas de código executadas durante a execução dos testes

## O que não é considerado
- Código comentado
- Métodos que apenas delegam responsabilidades sem lógica
- Arquivos de configuração

## Validação
A cobertura será verificada através do relatório `coverage.opencover.xml` gerado pela execução dos testes:

```powershell
dotnet test --collect:"XPlat Code Coverage"
```

## 15. Criar testes de integração com WebApplicationFactory
### Prompt
Criar testes de integração com WebApplicationFactory para validar o pipeline HTTP completo.

Adicionar casos de teste para:
- resposta do controlador;
- fallback local.

Expandir CI para executar esses testes e manter o limite de cobertura.

Requisitos:
- usar `Microsoft.AspNetCore.Mvc.Testing`;
- validar o endpoint `POST /api/reunioes/resumir`;
- substituir dependências externas por fakes/mocks nos testes;
- evitar chamadas reais para provedores de IA;
- garantir que o pipeline HTTP completo execute controller, model binding, serialização e DI;
- manter o gate mínimo de cobertura de 30%;
- garantir execução via GitHub Actions;
- rodar `dotnet test` com coleta de cobertura.
