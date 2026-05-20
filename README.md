# EasyMeet

EasyMeet e uma aplicacao local Windows para analise inteligente de reunioes com multiplos provedores de IA.

## Principais recursos
- Selecao dinamica de provedor (`Gemini` ou `Groq`) em tempo de execucao
- Gerenciamento seguro de API keys via Windows Credential Manager
- Analise estruturada com:
  - resumo
  - topicos principais
  - acoes
  - responsaveis
  - decisoes
  - pendencias
  - tipo de reuniao
  - nivel de confianca

## Arquitetura
- `Controllers`: endpoints HTTP
- `Services`: orquestracao, providers, credenciais, factory
- `Models`: contratos de request/response e enums
- `Prompts`: template de prompt
- `wwwroot`: interface web local

## Segurança de credenciais
- Chaves nao sao salvas em `appsettings`, `launchSettings`, arquivos locais ou logs
- Persistencia por provedor no Windows Credential Manager:
  - `EasyMeet:Gemini`
  - `EasyMeet:Groq`

## Endpoints
- `POST /api/reunioes/resumir`
- `GET /api/ia/provedores`
- `GET /api/ia/credenciais/status`
- `POST /api/ia/credenciais`
- `DELETE /api/ia/credenciais/{provedor}`
- `POST /api/ia/credenciais/testar`

## Exemplo de request de analise
```json
{
  "transcricao": "Conteudo da reuniao...",
  "provedorIA": "Gemini"
}
```

## Build, testes e execucao
```powershell
dotnet build EasyMeet.sln
dotnet test EasyMeet.sln
dotnet run --project .\src\EasyMeet.Api\EasyMeet.Api.csproj
```
