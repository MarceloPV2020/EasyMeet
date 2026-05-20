# Viabilidade Técnica - EasyMeet

## Plataforma alvo
- Sistema operacional: Windows.
- Runtime: .NET 9.
- Backend: ASP.NET Core Web API.
- Frontend: HTML/CSS/JavaScript servido pela própria API.
- Armazenamento de credenciais: Windows Credential Manager.

## Viabilidade dos provedores
- Gemini: viável via API oficial `generativelanguage.googleapis.com`.
- Groq: viável via API compatível com chat completions.
- OpenAI: viável via `v1/chat/completions`.
- Anthropic: viável via `v1/messages`.
- Mistral: viável via `v1/chat/completions`.
- Cohere: viável via `v2/chat`.
- AzureOpenAI: viável com endpoint, deployment e API version configurados.

## Condições externas
A disponibilidade de cada provedor depende de:
- chave válida;
- billing ativo quando exigido;
- quota disponível;
- modelo ou deployment habilitado na conta;
- disponibilidade temporária do serviço externo.

## Segurança de credenciais
- O uso do Windows Credential Manager é adequado para uma aplicação local Windows.
- Cada chave é armazenada por provedor, usando o padrão `EasyMeet:{ProvedorIA}`.
- A solução evita persistência de API keys em arquivos versionados ou logs.

## Riscos técnicos
- Quota excedida ou limite de requisições (`429`).
- Alta demanda temporária do provedor (`503`).
- Chave expirada ou inválida.
- Modelo descontinuado ou indisponível.
- Configuração incorreta de AzureOpenAI.
- Mudanças futuras nas APIs externas.

## Mitigações implementadas
- Mensagens amigáveis com detalhe técnico preservado.
- Atalho para troca de provedor quando a quota do Gemini é excedida.
- Execução estrita no modelo selecionado para facilitar diagnóstico.
- Isolamento de provedores por implementação de `IGenerativeAIClient`.
- Parser centralizado para validar e normalizar a resposta da IA.

## Avaliação de custo-benefício
- Benefício: reduz esforço manual para transformar transcrições em atas e planos de ação.
- Benefício: permite comparar custo, qualidade e disponibilidade entre provedores.
- Custo: depende do consumo de tokens e das regras comerciais de cada provedor.
- Risco controlável: quotas e billing são externos, mas a aplicação comunica falhas de forma clara.

## Conclusão
A solução é tecnicamente viável para uso local em Windows. A arquitetura atual suporta expansão de provedores, mantém credenciais protegidas e oferece uma experiência adequada para análise real de reuniões com IA.
