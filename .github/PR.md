# Resumo

Refatora o EasyMeet para uma arquitetura multi-provedor de IA, com credenciais seguras por provedor, seleção dinâmica em tempo de execução, configurações avançadas de inferência e documentação técnica atualizada.

# Tipo de alteração
- [x] Funcionalidade
- [x] Correção
- [x] Testes
- [x] Documentação
- [x] Refatoração
- [x] Segurança

# O que foi implementado
- Suporte a Gemini, Groq, OpenAI, Anthropic, Mistral, Cohere e AzureOpenAI.
- Organização dos providers em `src/EasyMeet.Api/Providers`.
- `ProvedorIA`, `IGenerativeAIClient`, `IAProviderFactory` e clients concretos por provedor.
- Armazenamento de API keys no Windows Credential Manager via `WindowsCredentialApiKeyStore`.
- Endpoints `/api/ia/*` para listar provedores, salvar, testar, remover e consultar credenciais.
- Análise de reunião sem envio de API key no request.
- Configurações avançadas por análise: modelo, temperatura e máximo de tokens.
- UI com menu lateral de configuração, defaults por provedor e mensagens amigáveis com detalhe técnico.
- Prompt e parser atualizados para resposta estruturada em JSON válido.
- Documentação profissional em português: ADR, Backlog, Diretrizes de IA, PRD, Viabilidade, UML e prompts.
- Testes automatizados para fluxo multi-provedor, credenciais e análise.

# Como testar
```powershell
dotnet build EasyMeet.sln
dotnet test EasyMeet.sln
dotnet run --project .\src\EasyMeet.Api\EasyMeet.Api.csproj
```

# Evidências
- `dotnet test EasyMeet.sln`: 11 testes aprovados, 0 falhas.

# Checklist
- [x] Sem persistência de API key em arquivos locais
- [x] Sem retorno de API key em endpoints
- [x] Windows Credential Manager usado para credenciais
- [x] Multi-provedor registrado por DI
- [x] Providers organizados em pasta própria
- [x] Interface web atualizada
- [x] Documentação atualizada
- [x] Testes automatizados atualizados
- [x] Swagger/API preservados

# Observações
- OpenAI, Anthropic e AzureOpenAI dependem de chaves, billing, quota e permissões externas.
- AzureOpenAI também depende de endpoint, deployment e API version configurados corretamente.
