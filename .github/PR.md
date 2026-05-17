# Resumo
Descreva de forma objetiva o que este PR entrega, qual problema resolve e qual impacto esperado no EasyMeet.

## Contexto
Explique o cenário que motivou a alteração (requisito acadêmico, bug, melhoria técnica ou evolução funcional).

# Tipo de alteração
- [ ] Funcionalidade
- [ ] Correção
- [ ] Testes
- [ ] Documentação
- [ ] Refatoração

# O que foi implementado
Liste os principais itens implementados neste PR.

Exemplo:
- Endpoint/serviço/modelo adicionado ou ajustado.
- Regras de validação incluídas.
- Integração com Gemini/fallback atualizada.
- Documentação e testes atualizados.

# Como testar
## Pré-requisitos
- .NET 9 SDK instalado
- (Opcional) `GEMINI_API_KEY` configurada para validação da IA real

## Passo a passo
1. Restaurar/buildar projeto:
   - `dotnet build EasyMeet.sln`
2. Executar testes:
   - `dotnet test .\tests\EasyMeet.Tests\EasyMeet.Tests.csproj`
3. Subir API:
   - `dotnet run --project .\src\EasyMeet.Api\EasyMeet.Api.csproj`
4. Validar endpoint em `/swagger` e interface web em `/`.

# Evidências
Anexe evidências objetivas:
- prints do Swagger/interface;
- saída de `dotnet test`;
- exemplo de request/response;
- logs relevantes (sem segredos).

# Requisitos da atividade atendidos
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

# Checklist técnico
- [ ] Build local sem erros
- [ ] Testes passando
- [ ] Sem quebra de contrato da API
- [ ] Sem segredos/versionamento de chaves
- [ ] Logs e tratamento de falhas adequados

# Riscos e impactos
Descreva riscos conhecidos, possíveis regressões e impacto em componentes existentes.

# Observações
Inclua informações adicionais para o revisor (trade-offs, decisões técnicas e próximos passos).
