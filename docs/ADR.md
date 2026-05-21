# ADR - Architecture Decision Records

Este documento registra as principais decisões arquiteturais do EasyMeet, seus motivos e trade-offs.

## ADR-001 - ASP.NET Core .NET 9 Como Base da API

Status: Aceita

Decisão: usar ASP.NET Core Web API em .NET 9 para implementar endpoints REST, Swagger, injeção de dependência e integrações HTTP.

Motivos:

- boa produtividade para APIs;
- suporte nativo a DI e `HttpClientFactory`;
- facilidade de testes automatizados;
- aderência ao escopo acadêmico de engenharia de software.

Trade-offs:

- exige runtime .NET instalado;
- frontend permanece simples, sem framework SPA dedicado.

## ADR-002 - Arquitetura Multi-Provider

Status: Aceita

Decisão: manter múltiplos providers reais de IA por meio do contrato `IGenerativeAIClient`.

Motivos:

- reduzir lock-in;
- comparar qualidade, latência e custo;
- permitir que o usuário escolha o provider;
- manter OpenRouter e APIs diretas coexistindo.

Trade-offs:

- maior superfície de manutenção;
- cada provider tem limites, formatos e erros próprios.

## ADR-003 - OpenRouter Como Provider Agregador

Status: Aceita

Decisão: implementar `OpenRouterClientService` além dos providers diretos.

Motivos:

- acesso a vários modelos por uma única chave;
- facilidade para experimentar modelos;
- útil para contas com créditos ou rotas variadas.

Trade-offs:

- disponibilidade de modelos varia por conta;
- alguns modelos podem retornar `404 No endpoints found`;
- custo e limites dependem da política do OpenRouter.

## ADR-004 - Manutenção Dos Providers Diretos

Status: Aceita

Decisão: não simplificar a arquitetura para apenas OpenRouter.

Motivos:

- APIs oficiais oferecem controle direto;
- providers diretos reduzem dependência de agregador;
- comparação técnica entre integrações faz parte do valor do projeto.

## ADR-005 - Separação de Providers

Status: Aceita

Decisão: cada provider possui sua própria classe em `Providers/`.

Motivos:

- isolar regras de payload;
- isolar tratamento de erros;
- facilitar inclusão/remoção de modelos;
- manter `AgenteResumoReuniaoService` livre de detalhes de API externa.

## ADR-006 - Factory para Resolução do Provider

Status: Aceita

Decisão: usar `AIProviderFactory`/`IAProviderFactory` para resolver o cliente correto a partir de `ProvedorIA`.

Motivos:

- reduzir condicionais no fluxo principal;
- centralizar resolução de providers;
- melhorar testabilidade.

## ADR-007 - Credenciais No Windows Credential Manager

Status: Aceita

Decisão: armazenar API keys via `IApiKeyStore` e `WindowsCredentialApiKeyStore`.

Motivos:

- evitar segredos em arquivos;
- permitir chaves diferentes por provider;
- manter a aplicação adequada ao uso local Windows.

Trade-offs:

- dependência operacional do ambiente Windows para armazenamento seguro local;
- testes usam fakes para evitar acesso real a credenciais.

## ADR-008 - Execução Estrita do Modelo Selecionado

Status: Aceita

Decisão: executar exatamente o modelo selecionado pelo usuário, ou o padrão do provider quando nenhum modelo for informado.

Motivos:

- previsibilidade;
- rastreabilidade do histórico;
- transparência quando um modelo falha.

Trade-offs:

- não há fallback automático para outro modelo;
- usuário precisa escolher outro modelo em caso de erro.

## ADR-009 - Histórico Local Em SQLite

Status: Aceita

Decisão: persistir análises em SQLite.

Motivos:

- simples para aplicação local;
- dispensa infraestrutura externa;
- permite filtros e exportação posterior;
- facilita demonstração acadêmica.

Trade-offs:

- não resolve colaboração multiusuário;
- não substitui banco centralizado para uso corporativo amplo.

## ADR-010 - Exportação PDF No Frontend

Status: Aceita

Decisão: usar jsPDF para gerar relatórios a partir do histórico.

Motivos:

- evita endpoint adicional para PDF;
- reduz complexidade backend;
- permite exportação imediata pelo usuário.

Trade-offs:

- layout de PDF depende do navegador;
- relatórios avançados podem exigir geração server-side no futuro.

## ADR-011 - Parser Estruturado para Resposta da IA

Status: Aceita

Decisão: validar e normalizar o JSON retornado pelos LLMs em `MeetingAnalysisParser`.

Motivos:

- modelos variam em aderência ao prompt;
- a API precisa entregar contrato estável;
- erros de JSON devem ser tratados de forma clara.

## ADR-012 - GitHub Actions como Gate de Qualidade

Status: Aceita

Decisão: executar restore, build e testes em `push` e `pull_request`.

Motivos:

- automatizar validação;
- proteger a branch principal;
- evidenciar boas práticas de engenharia.
