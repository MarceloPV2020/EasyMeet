# PRD - EasyMeet

## 1. Visão geral
EasyMeet é uma API REST para transformar transcrições de reuniões em informações estruturadas, apoiando acompanhamento de decisões, responsabilidades e próximos passos.

## 2. Objetivo do produto
Entregar uma solução de análise inteligente de reuniões que reduza esforço manual na documentação e aumente a clareza sobre ações e responsáveis.

## 3. Problema
Times realizam muitas reuniões e perdem tempo consolidando:
- resumo do que foi discutido;
- ações definidas;
- responsáveis por execução;
- contexto para acompanhamento.

Esse processo manual é lento, inconsistente e propenso a erros.

## 4. Público-alvo
- equipes de desenvolvimento de software;
- líderes técnicos e gerentes de projeto;
- squads ágeis com rituais frequentes;
- contextos acadêmicos e laboratoriais de IA aplicada.

## 5. Solução proposta
API que recebe texto/transcrição e retorna JSON estruturado com:
- resumo;
- tópicos principais;
- ações;
- responsáveis;
- tipo de reunião;
- nível de confiança da análise.

A solução usa IA generativa (Gemini 2.5 Flash) e fallback local para garantir continuidade.

## 6. Funcionalidades
- endpoint `POST /api/reunioes/resumir`;
- validação de entrada (texto e idioma);
- geração de prompt estruturado;
- integração com Gemini API via `HttpClient`;
- validação e desserialização segura do retorno da IA;
- fallback local automático em falhas;
- documentação Swagger;
- testes automatizados xUnit.

## 7. Papel da IA
A IA é componente funcional central do produto:
- sintetiza conteúdo de reunião;
- identifica itens acionáveis;
- identifica possíveis responsáveis;
- classifica o tipo de reunião;
- estima confiança da análise.

Sem IA, a proposta principal de valor do EasyMeet fica limitada.

## 8. Requisitos funcionais
- RF01: receber payload com `texto` e `idioma`.
- RF02: validar texto vazio.
- RF03: validar texto muito curto.
- RF04: processar texto com IA para gerar saída estruturada.
- RF05: retornar campos obrigatórios no formato definido.
- RF06: indicar se a saída veio de IA real (`geradoPorIA = true`) ou fallback.
- RF07: retornar `modoExecucao` (`gemini` ou `fallback_local`).
- RF08: manter endpoint assíncrono com `CancellationToken`.
- RF09: expor documentação do endpoint via Swagger.

## 9. Requisitos não funcionais
- RNF01: desempenho adequado para requisições síncronas de API.
- RNF02: resiliência a falhas externas (timeout, erro HTTP, JSON inválido).
- RNF03: disponibilidade com fallback local.
- RNF04: código organizado, legível e com separação de responsabilidades.
- RNF05: segurança básica de segredos (uso de variável de ambiente).
- RNF06: testabilidade com mocks/fakes sem dependência de rede.

## 10. Regras de negócio
- RN01: texto deve conter conteúdo mínimo para análise.
- RN02: resposta deve respeitar contrato JSON obrigatório.
- RN03: `nivelConfianca` deve ficar entre `0.0` e `1.0`.
- RN04: quando IA falhar, fallback local deve ser aplicado automaticamente.
- RN05: não inventar informações sem evidência no texto.
- RN06: quando não houver evidência para classificação, usar `Unknown`.

## 11. Limitações
- qualidade depende da clareza da transcrição recebida;
- fallback local possui inteligência limitada;
- não há persistência de histórico nesta versão;
- não há autenticação/autorização implementada;
- classificação pode perder nuances em textos muito curtos ou ambíguos.

## 12. Roadmap futuro
- persistência de análises em banco de dados;
- autenticação e controle de acesso;
- upload de áudio com transcrição automática;
- dashboard com indicadores de reuniões;
- métricas de qualidade da IA e taxa de fallback;
- testes de integração end-to-end e contratos;
- suporte avançado a múltiplos idiomas;
- melhoria de prompts e avaliação contínua de precisão.
