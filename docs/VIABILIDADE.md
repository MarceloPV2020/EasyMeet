# Estudo de Viabilidade Técnica - EasyMeet

## 1. Plataforma Alvo
* **Sistema Operacional:** Windows, Linux e macOS (Suporte Multiplataforma via .NET)
* **Runtime:** .NET 9
* **Backend:** ASP.NET Core Web API
* **Frontend:** HTML/CSS/JavaScript integrado ou cliente SPA segregado.
* **Armazenamento de Credenciais:** Integração segura de chaves de API por meio de variáveis de ambiente e gerenciadores de segredos (Secret Manager/Azure Key Vault).

## 2. Viabilidade dos Provedores de IA
O sistema EasyMeet foi validado tecnicamente para consumir os principais provedores de modelos de linguagem (LLMs) do mercado através de chamadas HTTP seguras:

* **Gemini:** Viável via API oficial `generativelanguage.googleapis.com`.
* **Groq:** Viável via API compatível com o padrão OpenAI.
* **OpenAI:** Viável via endpoints oficiais `v1/chat/completions`.
* **Anthropic:** Viável via API Anthropic `v1/messages`.
* **Mistral:** Viável via endpoints `/v1/chat/completions`.
* **Cohere:** Viável via endpoints `/v2/chat`.
* **Azure OpenAI:** Viável com suporte a deployments, endpoints customizados e controle de versão de API específicos da nuvem Microsoft.

## 3. Requisitos e Condições Externas
A estabilidade e integração de cada provedor no ecossistema EasyMeet dependem diretamente de:
* Fornecimento de uma chave de API (API Key) válida e ativa.
* Configuração correta de faturamento (Billing) junto ao provedor, caso aplicável.
* Respeito aos limites de requisições vigentes (Quota/Rate Limits).
* Existência do modelo ou deployment configurado de forma idêntica no painel do provedor.