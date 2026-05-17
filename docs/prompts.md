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

## 11. Criar template do Pull Request
### Prompt
Crie o arquivo .github/PR.md.

O template deve conter:

# Resumo
# Tipo de alteração
- [ ] Funcionalidade
- [ ] Correção
- [ ] Testes
- [ ] Documentação
- [ ] Refatoração

# O que foi implementado
# Como testar
# Evidências
# Requisitos da atividade atendidos
# Observações

Incluir checklist:
- [ ] README.md completo
- [ ] PRD.md
- [ ] VIABILIDADE.md
- [ ] BACKLOG.md
- [ ] UML.md
- [ ] ADR.md
- [ ] DIRETRIZES_IA.md
- [ ] prompts.md
- [ ] Swagger funcional
- [ ] IA integrada ao produto
- [ ] Gemini funcionando
- [ ] 5 testes automatizados
- [ ] Pull Request aberto

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
