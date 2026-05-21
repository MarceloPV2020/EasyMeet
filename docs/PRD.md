# PRD - EasyMeet

## Visão do Produto

EasyMeet é uma plataforma inteligente para transformar transcrições de reuniões em informações estruturadas, acionáveis e persistidas em histórico local. O produto combina ASP.NET Core, interface web e integração real com modelos de linguagem para apoiar tomada de decisão, acompanhamento de tarefas e registro de conhecimento.

## Objetivo

Criar uma experiência simples para que usuários colem uma transcrição, escolham um provedor/modelo de IA e recebam uma análise estruturada com resumo, tópicos, ações, responsáveis, decisões, pendências, classificação e confiança.

## Problema

Atas e registros manuais de reunião costumam ser inconsistentes, demorados e difíceis de consultar. Informações como responsáveis, decisões e pendências podem se perder em conversas longas. O EasyMeet reduz esse atrito usando IA para estruturar o conteúdo.

## Público-Alvo

- equipes administrativas;
- líderes e gestores;
- equipes de projeto;
- ambientes acadêmicos;
- profissionais que precisam registrar decisões e encaminhamentos;
- usuários que desejam comparar diferentes provedores de IA.

## Solução Proposta

Uma aplicação web local com API REST que:

- recebe texto de reunião;
- executa um agente de IA real;
- permite selecionar provider e modelo;
- estrutura o resultado em campos separados;
- salva o histórico;
- permite consultar análises anteriores;
- exporta resultados em PDF.

## MVP Concluído

- API ASP.NET Core .NET 9.
- Swagger funcional.
- Interface web local.
- Múltiplos providers de IA.
- OpenRouter e providers diretos coexistindo.
- Armazenamento seguro de API keys.
- Análise estruturada de reuniões.
- Histórico persistido em SQLite.
- Exportação PDF.
- Testes automatizados.
- GitHub Actions.

## Funcionalidades

- Configurar, testar e remover API keys por provedor.
- Selecionar provider de IA para análise.
- Selecionar modelo, temperatura e máximo de tokens.
- Salvar preferências locais por provedor.
- Filtrar modelos OpenRouter por disponibilidade da chave configurada.
- Enviar transcrição para análise.
- Exibir resumo, tópicos, ações, responsáveis, decisões, pendências, data, tipo e confiança.
- Registrar provider e modelo utilizado no histórico.
- Filtrar histórico por texto, data e tipo.
- Remover itens do histórico.
- Exportar análise em PDF.

## Requisitos Funcionais

- RF01: permitir seleção dinâmica de provider de IA.
- RF02: permitir configuração segura de API keys por provider.
- RF03: validar transcrição vazia ou curta.
- RF04: executar a análise usando chamada real a LLM.
- RF05: suportar OpenRouter e providers diretos.
- RF06: respeitar modelo, temperatura e maxTokens informados pelo usuário.
- RF07: retornar resposta estruturada em JSON.
- RF08: persistir análises no histórico local.
- RF09: exibir histórico com provider e modelo utilizados.
- RF10: permitir exportação PDF.
- RF11: manter Swagger disponível para exploração da API.
- RF12: executar build e testes no GitHub Actions.

## Requisitos Não Funcionais

- RNF01: API keys não devem ser versionadas, logadas ou expostas em responses.
- RNF02: a arquitetura deve permitir adicionar novos providers com baixo acoplamento.
- RNF03: chamadas externas devem usar `async/await` e `CancellationToken`.
- RNF04: erros de provider devem preservar detalhe técnico útil.
- RNF05: a interface deve ser responsiva e adequada ao uso recorrente.
- RNF06: a aplicação deve ser testável com `dotnet test`.
- RNF07: histórico deve ser persistido localmente em SQLite.

## Fluxo do Usuário

1. Usuário abre a aplicação web.
2. Configura a API key do provider desejado.
3. Escolhe provider e, opcionalmente, modelo/temperatura/tokens.
4. Cola a transcrição.
5. Solicita a análise.
6. O backend busca a credencial segura.
7. O provider selecionado chama o LLM.
8. O parser valida o JSON retornado.
9. A aplicação exibe e salva o resultado.
10. Usuário consulta histórico ou exporta PDF.

## Integração IA

A IA é usada como motor funcional da aplicação. O prompt orienta o modelo a retornar JSON estruturado com campos obrigatórios. O backend valida o formato e normaliza campos para reduzir variações entre provedores.

## Arquitetura Multi-Provider

O sistema usa `IGenerativeAIClient` como contrato comum. Cada provider possui sua própria classe de integração HTTP, enquanto `AIProviderFactory` resolve o cliente correto conforme `ProvedorIA`.

Providers atuais:

- OpenRouter
- Gemini
- Groq
- OpenAI
- Anthropic
- Mistral
- Cohere

## Regras De Negócio

- A análise depende de chave configurada para o provider selecionado.
- O sistema executa exatamente o modelo selecionado ou o padrão do provider quando nenhum modelo é informado.
- Não há troca automática de provider/modelo em caso de erro.
- A IA não deve inventar informações ausentes na transcrição.
- Responsáveis, prazos e decisões devem ser extraídos apenas quando houver evidências.
- Histórico deve registrar provider e modelo usados.

## Fora Do Escopo Atual

- Login multiusuário.
- Upload de áudio.
- Transcrição automática.
- Edição colaborativa.
- Dashboard analítico.
- Sincronização em nuvem.

## Futuras Evoluções

- Upload de áudio e transcrição automática.
- Comparação lado a lado entre modelos.
- Métricas de custo, tokens, latência e taxa de erro.
- Autenticação e perfis.
- Compartilhamento de relatórios.
- Exportação DOCX/Markdown.
- Dashboard executivo.
