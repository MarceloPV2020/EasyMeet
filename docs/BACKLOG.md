# Backlog - EasyMeet

Este backlog consolida o estado atual da aplicação, as user stories geradas com apoio de IA e as próximas melhorias recomendadas.

## Funcionalidades Implementadas
- Arquitetura multi-provedor em tempo de execução.
- Provedores suportados: Gemini, Groq, OpenAI, Anthropic, Mistral, Cohere e AzureOpenAI.
- Armazenamento seguro de API keys no Windows Credential Manager.
- Tela de configuração de IA em menu lateral recolhível.
- Seleção de provedor para análise de reunião.
- Configurações avançadas por análise: modelo, temperatura e máximo de tokens.
- Preferências padrão por provedor salvas localmente no navegador.
- Último provedor utilizado sugerido automaticamente ao abrir a tela.
- Mensagens amigáveis com detalhe técnico preservado.
- Atalho visual para trocar para Groq quando a quota do Gemini for excedida.
- Parser de resposta estruturada com suporte ao schema atual e compatibilidade com formatos legados.

## User Stories

### US01 - Configurar Provedor de IA com Segurança
Como usuário,
Quero cadastrar, testar e remover a API key de cada provedor de IA,
Para utilizar a análise inteligente sem expor credenciais em arquivos ou logs.

Critérios de aceitação:
- O usuário deve conseguir selecionar um provedor de IA.
- O usuário deve conseguir salvar uma API key para o provedor selecionado.
- O usuário deve conseguir testar a conexão antes de analisar uma reunião.
- O usuário deve conseguir remover a chave salva.
- A API key não deve ser exibida, registrada em log ou armazenada em arquivos do projeto.
- O status do provedor deve indicar se a chave está configurada ou não configurada.

Tarefas técnicas:
- Implementar armazenamento via `WindowsCredentialApiKeyStore`.
- Expor endpoints em `IAConfigController`.
- Atualizar a UI de configuração em menu lateral.
- Criar mensagens amigáveis para falhas de autenticação, chave expirada e quota.
- Cobrir fluxo com testes unitários do `ApiKeyManagerService`.

Prioridade: Alta
Status: Implementado

Cenário BDD:
Dado que o usuário abriu a configuração das IAs,
Quando ele selecionar um provedor, informar uma API key válida e clicar em salvar,
Então a chave deve ser armazenada no Windows Credential Manager e o status deve ser exibido como configurado.

### US02 - Analisar Reunião com Provedor Selecionável
Como usuário,
Quero escolher o provedor de IA antes de analisar uma transcrição,
Para comparar qualidade, custo e disponibilidade entre diferentes modelos.

Critérios de aceitação:
- O usuário deve conseguir selecionar o provedor antes da análise.
- A análise deve usar somente a chave salva para o provedor selecionado.
- O endpoint de análise não deve receber API key no payload.
- A resposta deve exibir resumo, tópicos, ações, responsáveis, decisões, pendências, tipo de reunião e confiança.
- Se não houver chave configurada, a análise deve ser bloqueada com mensagem clara.
- A aplicação não deve usar fallback local para gerar uma análise válida.

Tarefas técnicas:
- Incluir `provedorIA` em `ResumoReuniaoRequest`.
- Resolver o cliente correto via `IAProviderFactory`.
- Buscar a chave no `IApiKeyStore`.
- Chamar `IGenerativeAIClient.GerarConteudoAsync`.
- Parsear e normalizar a resposta com `MeetingAnalysisParser`.
- Atualizar testes de `AgenteResumoReuniaoService` e `ReunioesController`.

Prioridade: Alta
Status: Implementado

Cenário BDD:
Dado que o usuário possui uma chave configurada para Groq,
Quando ele selecionar Groq, informar uma transcrição válida e clicar em analisar,
Então o sistema deve chamar o provedor Groq e exibir a resposta estruturada em campos separados.

### US03 - Ajustar Configurações Avançadas da IA
Como usuário,
Quero configurar modelo, temperatura e máximo de tokens por provedor,
Para controlar previsibilidade, tamanho da resposta e custo da análise.

Critérios de aceitação:
- O usuário deve visualizar modelos compatíveis com o provedor selecionado.
- O usuário deve escolher a temperatura por uma lista de opções claras.
- O usuário deve visualizar o valor sugerido para máximo de tokens.
- O usuário deve salvar padrões por provedor no navegador.
- Ao trocar o provedor, a tela deve carregar os padrões correspondentes.
- A análise deve respeitar o modelo selecionado sem trocar automaticamente para outro modelo.

Tarefas técnicas:
- Adicionar `ConfiguracaoAnaliseIA`.
- Atualizar `IGenerativeAIClient` para receber configuração por análise.
- Aplicar limites seguros em temperatura e tokens nos providers.
- Atualizar a UI de configurações avançadas.
- Persistir defaults por provedor no `localStorage`.
- Validar comportamento com testes unitários e execução manual.

Prioridade: Média
Status: Implementado

Cenário BDD:
Dado que o usuário selecionou Gemini como provedor,
Quando ele escolher o modelo `gemini-2.5-flash-lite`, temperatura `0.2` e salvar como padrão,
Então esses valores devem ser reaplicados automaticamente quando Gemini for selecionado novamente.

### US04 - Diagnosticar Erros de Provedor
Como usuário,
Quero receber mensagens claras quando a IA falhar,
Para entender se devo revisar a chave, trocar provedor, reduzir tokens ou tentar novamente depois.

Critérios de aceitação:
- Erros técnicos devem ser convertidos em mensagens amigáveis.
- O detalhe técnico original deve continuar disponível na interface.
- Erros de quota, rate limit, chave expirada, modelo inválido e indisponibilidade devem ser identificáveis.
- Quando a quota do Gemini for excedida, a UI deve sugerir troca para Groq.

Tarefas técnicas:
- Implementar mapeamento de mensagens no frontend.
- Retornar detalhe técnico no teste de conexão.
- Preservar mensagens originais vindas dos provedores.
- Adicionar teste para resposta detalhada no `ApiKeyManagerService`.

Prioridade: Alta
Status: Implementado

Cenário BDD:
Dado que o provedor Gemini retornou erro de quota excedida,
Quando o usuário tentar analisar uma reunião,
Então a interface deve exibir uma mensagem amigável, preservar o detalhe técnico e oferecer a opção de trocar para Groq.

## Itens Técnicos Priorizados

### P1 - Diagnóstico estruturado de erros por provedor
- Objetivo: retornar códigos internos para erros conhecidos (`quota_excedida`, `chave_expirada`, `modelo_invalido`, `rate_limit`, `timeout`).
- Benefício: simplifica tratamento na UI e testes automatizados.
- Status: Planejado

### P1 - Cobertura de testes ampliada
- Objetivo: cobrir endpoints de configuração de IA, teste detalhado de conexão e política de execução estrita por modelo.
- Benefício: reduz risco ao adicionar novos provedores.
- Status: Em andamento

### P1 - Configuração assistida para AzureOpenAI
- Objetivo: orientar o preenchimento de endpoint, deployment e API version.
- Benefício: reduz erros operacionais comuns no Azure.
- Status: Planejado

### P2 - Guia operacional por provedor
- Objetivo: documentar pré-requisitos, modelos recomendados, erros comuns e ações corretivas.
- Benefício: acelera implantação e suporte.
- Status: Planejado

### P2 - Observabilidade sem exposição de segredos
- Objetivo: registrar metadados seguros das chamadas, como provedor, modelo, duração e status.
- Benefício: facilita análise de falhas sem risco de vazamento de API keys.
- Status: Planejado

### P3 - Testes de contrato por provedor
- Objetivo: criar testes opt-in com chaves reais em ambiente controlado.
- Benefício: valida compatibilidade com APIs externas sem comprometer a suíte local.
- Status: Futuro
