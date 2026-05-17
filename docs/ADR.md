# ADR - EasyMeet

## ADR-001: Provedor de IA (Gemini 2.5 Flash)
Problema: escolher um modelo para análise textual de reuniões com boa latência.
Decisão: usar Google Gemini 2.5 Flash via API HTTP.
Justificativa: equilíbrio entre custo, velocidade e capacidade de extração estruturada.
Consequências: dependência externa mitigada por fallback local.

## ADR-002: Fallback local obrigatório
Problema: garantir resposta mesmo com falha da IA externa.
Decisão: implementar resposta local padronizada quando a integração falhar.
Justificativa: manter disponibilidade e contrato da API.
Consequências: qualidade semântica inferior no fallback, porém maior robustez.

## ADR-003: Plataforma ASP.NET Core .NET 9
Problema: definir stack backend para API acadêmica com alta produtividade.
Decisão: ASP.NET Core Web API em .NET 9.
Justificativa: ecossistema maduro, DI nativa, Swagger e suporte a testes.
Consequências: projeto alinhado a práticas modernas de desenvolvimento C#.

## ADR-004: Separação em Controllers/Services/Models/Prompts
Problema: evitar acoplamento e facilitar manutenção.
Decisão: separar entrada HTTP, regra de negócio, contratos e prompt engineering.
Justificativa: arquitetura limpa e legível para evolução incremental.
Consequências: melhor testabilidade e organização acadêmica.

## ADR-005: Prompt engineering com contrato JSON estrito
Problema: reduzir respostas inválidas e alucinações.
Decisão: prompt com schema explícito e instrução de retorno somente em JSON.
Justificativa: aumenta previsibilidade da saída do modelo.
Consequências: exige validação robusta pós-resposta.

## ADR-006: Testes automatizados com fakes/mocks
Problema: validar comportamento sem depender da API real do Gemini.
Decisão: usar xUnit + fakes de HttpMessageHandler e serviços.
Justificativa: testes rápidos, determinísticos e sem custo externo.
Consequências: complementação futura com testes de integração real controlados.
