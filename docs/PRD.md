# PRD - EasyMeet

## Visão geral
O EasyMeet é uma aplicação local para Windows voltada à análise inteligente de reuniões. A solução permite configurar múltiplos provedores de IA, armazenar credenciais com segurança e gerar uma resposta estruturada a partir de uma transcrição.

## Objetivo
Entregar uma experiência profissional para análise de reuniões com:
- seleção de provedor de IA em tempo de execução;
- gerenciamento seguro de API keys;
- análise real com IA, sem fallback local;
- visualização clara de resumo, tópicos, ações, responsáveis, decisões, pendências, tipo de reunião e confiança;
- configuração avançada de modelo, temperatura e máximo de tokens.

## Público-alvo
- equipes administrativas e corporativas;
- profissionais que registram atas, ações e decisões;
- instrutores, coordenadores e gestores que precisam transformar transcrições em informação operacional.

## Funcionalidades implementadas
- Configuração de provedores em menu lateral recolhível.
- Cadastro, substituição, remoção e teste de API key por provedor.
- Status visual de credencial configurada ou não configurada.
- Seleção de provedor para análise.
- Persistência local do último provedor utilizado.
- Configurações avançadas por provedor.
- Defaults avançados salvos no navegador por provedor.
- Mensagens de erro amigáveis com detalhe técnico.
- Atalho para trocar para Groq quando a quota do Gemini for excedida.

## Provedores suportados
- Gemini
- Groq
- OpenAI
- Anthropic
- Mistral
- Cohere
- AzureOpenAI

## Requisitos funcionais
- RF01: o usuário deve selecionar o provedor de IA para análise.
- RF02: o usuário deve salvar, substituir, testar e remover API keys por provedor.
- RF03: a aplicação deve bloquear a análise quando não houver chave configurada para o provedor selecionado.
- RF04: a aplicação deve enviar ao endpoint de análise somente transcrição, provedor e configurações de IA, sem API key.
- RF05: a aplicação deve retornar resposta estruturada em campos separados.
- RF06: o usuário deve poder configurar modelo, temperatura e máximo de tokens.
- RF07: a aplicação deve executar a análise no modelo selecionado ou no padrão do provedor.
- RF08: a aplicação deve apresentar erros de forma amigável sem ocultar detalhes técnicos úteis.

## Requisitos não funcionais
- RNF01: API keys devem ser armazenadas no Windows Credential Manager.
- RNF02: API keys não devem aparecer em logs, arquivos de configuração ou respostas HTTP.
- RNF03: a arquitetura deve permitir inclusão de novos provedores sem alterar o fluxo principal.
- RNF04: a API deve usar `async/await` e `CancellationToken`.
- RNF05: a interface deve ser responsiva, clara e adequada ao uso recorrente.
- RNF06: a aplicação deve ser validável com `dotnet build EasyMeet.sln` e `dotnet test EasyMeet.sln`.

## APIs principais
- `GET /api/ia/provedores`
- `GET /api/ia/credenciais/status`
- `POST /api/ia/credenciais`
- `DELETE /api/ia/credenciais/{provedor}`
- `POST /api/ia/credenciais/testar`
- `POST /api/reunioes/analisar`

## Fluxo de análise
1. O usuário seleciona um provedor.
2. O usuário informa ou mantém as configurações avançadas.
3. O usuário envia a transcrição para análise.
4. O backend busca a chave salva no Windows Credential Manager.
5. O backend seleciona o provider via `IAProviderFactory`.
6. O provider chama a IA real usando o modelo/configuração definida.
7. O parser valida e normaliza a resposta JSON.
8. A interface exibe cada informação em seu campo.

## Fora de escopo atual
- Transcrição automática de áudio.
- Login multiusuário.
- Armazenamento histórico de reuniões.
- Fallback local para análise.
- Roteamento automático entre provedores.
