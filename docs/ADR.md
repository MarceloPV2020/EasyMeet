# ADR - Registros de Decisão de Arquitetura

Este documento registra as principais decisões arquiteturais do EasyMeet e o motivo de cada escolha.

## ADR-001 - Arquitetura com múltiplos provedores de IA
- Status: Aceita
- Decisão: adotar a interface `IGenerativeAIClient` como contrato único para provedores de IA.
- Decisão: usar `IAProviderFactory` / `AIProviderFactory` para resolver o cliente correto a partir de `ProvedorIA`.
- Contexto: a aplicação precisa permitir seleção dinâmica de IA em tempo de execução sem alterar o fluxo central de análise de reuniões.
- Consequências: novos provedores podem ser adicionados com classes isoladas, registro em DI e configuração própria.
- Consequências: o serviço de análise permanece estável mesmo com APIs externas diferentes.

## ADR-002 - Armazenamento seguro de credenciais no Windows
- Status: Aceita
- Decisão: armazenar API keys no Windows Credential Manager por meio de `IApiKeyStore` e `WindowsCredentialApiKeyStore`.
- Decisão: usar o padrão de chave `EasyMeet:{ProvedorIA}`.
- Contexto: o EasyMeet é uma aplicação local Windows e não deve persistir segredos em arquivos do projeto.
- Consequências: API keys não ficam em `appsettings.json`, `launchSettings.json`, arquivos locais, logs ou repositório.
- Consequências: cada provedor possui ciclo de vida próprio para salvar, substituir, consultar e remover credenciais.

## ADR-003 - Seleção de provedor em tempo de execução
- Status: Aceita
- Decisão: incluir `provedorIA` no contrato de análise (`ResumoReuniaoRequest`).
- Decisão: permitir seleção de provedor na interface web para configuração e análise.
- Contexto: o usuário precisa comparar provedores, alternar em caso de quota/erro e escolher o melhor custo-benefício.
- Consequências: a mesma transcrição pode ser analisada por provedores diferentes.
- Consequências: UI e API precisam manter estado explícito do provedor selecionado.

## ADR-004 - Ausência de fallback local
- Status: Aceita
- Decisão: uma análise válida depende sempre de chamada real a um provedor de IA.
- Contexto: o produto é uma plataforma de análise inteligente e não deve produzir resultados simulados.
- Consequências: ausência de chave, chave inválida ou falha do provedor bloqueiam a análise.
- Consequências: erros devem ser comunicados de forma clara ao usuário.

## ADR-005 - Execução estrita no modelo selecionado
- Status: Aceita
- Decisão: executar a análise sempre no modelo selecionado pelo usuário ou no modelo padrão do provedor quando nenhum modelo for informado.
- Decisão: não trocar automaticamente para outro modelo em caso de erro.
- Contexto: o usuário pediu previsibilidade e controle explícito do modelo utilizado.
- Consequências: falhas ficam mais transparentes e fáceis de diagnosticar.
- Consequências: a aplicação evita resultados gerados por um modelo diferente daquele escolhido.

## ADR-006 - Mensagens amigáveis com diagnóstico técnico preservado
- Status: Aceita
- Decisão: mapear erros comuns para mensagens compreensíveis na interface.
- Decisão: preservar o detalhe técnico original do provedor quando disponível.
- Contexto: usuários não técnicos precisam de orientação objetiva, enquanto suporte/desenvolvimento precisam do erro real.
- Consequências: erros de quota, rate limit, chave expirada, modelo inválido e indisponibilidade ficam mais acionáveis.

## ADR-007 - Configurações avançadas por análise
- Status: Aceita
- Decisão: permitir que o usuário informe `modelo`, `temperatura` e `maxTokens` por análise.
- Decisão: permitir salvar preferências padrão por provedor no armazenamento local do navegador.
- Contexto: cada provedor possui modelos e limites diferentes; o usuário precisa ajustar custo, qualidade e tamanho da resposta.
- Consequências: a UI precisa exibir opções compatíveis por provedor.
- Consequências: o backend aplica limites seguros antes de chamar cada API externa.

## ADR-008 - Catalogo de modelos com disponibilidade por conta e sem diagnostico dedicado
- Status: Aceita
- Decisao: manter endpoint de verificacao de disponibilidade de modelos OpenRouter por chave configurada.
- Decisao: remover endpoint e UI de diagnostico dedicado para reduzir complexidade operacional.
- Contexto: a deteccao de disponibilidade resolve o principal problema de modelos com `404 No endpoints found` no OpenRouter.
- Consequencias: o usuario passa a operar a partir do catalogo filtrado da propria conta.
- Consequencias: o fluxo de analise fica mais simples e com menos pontos de suporte.
