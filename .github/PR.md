# Pull Request — EasyMeet

## Resumo

Este Pull Request implementa o projeto EasyMeet, uma aplicação desenvolvida em ASP.NET Core .NET 9 que utiliza Inteligência Artificial para análise automática de reuniões.

A aplicação recebe transcrições de reuniões e utiliza Google Gemini para:
- gerar resumo;
- identificar tópicos principais;
- identificar ações;
- identificar responsáveis;
- classificar automaticamente a reunião.

Além da integração real com IA, o projeto possui fallback local, testes automatizados, documentação completa e interface web amigável.

---

# Contexto

Este projeto foi desenvolvido como atividade acadêmica de IA para desenvolvimento de software, com foco em:
- uso funcional de IA no produto;
- arquitetura organizada;
- documentação técnica;
- testes automatizados;
- fluxo Git profissional com Pull Request.

# Tipo de alteração

- [x] Funcionalidade
- [x] Testes
- [x] Documentação
- [x] Refatoração

# O que foi implementado

## Backend/API
- Criação da API REST em ASP.NET Core .NET 9.
- Endpoint `POST /api/reunioes/resumir`.
- Integração com Google Gemini.
- Implementação de fallback local.
- Tratamento de erros e validações.

## IA
- Agente inteligente para análise de reuniões.
- Prompt engineering estruturado.
- Extração automática de:
  - resumo;
  - tópicos;
  - ações;
  - responsáveis;
  - classificação da reunião.

## Front-end
- Interface web simples em `wwwroot/index.html`.
- Consumo da API via JavaScript/fetch.

## Testes
- Testes automatizados xUnit.
- Cobertura de cenários principais e fallback.

## Documentação
- README.md
- PRD.md
- VIABILIDADE.md
- BACKLOG.md
- UML.md
- ADR.md
- DIRETRIZES_IA.md
- prompts.md

# Como testar

## Pré-requisitos

- .NET 9 SDK
- Chave Gemini configurada:

```powershell
$env:GEMINI_API_KEY="SUA_CHAVE"