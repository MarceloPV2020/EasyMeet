# Viabilidade Técnica - EasyMeet

## Resumo Executivo

O EasyMeet é tecnicamente viável porque combina tecnologias maduras: ASP.NET Core .NET 9, chamadas HTTP para LLMs, SQLite local, Swagger, testes automatizados e GitHub Actions. A arquitetura multi-provider reduz dependência de um único fornecedor e permite comparar custo, qualidade, latência e disponibilidade entre modelos.

## Viabilidade Técnica

### Backend

ASP.NET Core Web API é adequado ao projeto por oferecer:

- endpoints REST;
- injeção de dependência;
- `HttpClientFactory`;
- suporte a Swagger;
- execução assíncrona;
- boa testabilidade com xUnit.

### Frontend

A interface em HTML, CSS e JavaScript é suficiente para o escopo atual. Ela atende o uso local, reduz complexidade de build frontend e permite iteração rápida.

### Persistência

SQLite é viável para histórico local porque:

- não exige servidor externo;
- funciona bem para volume moderado;
- facilita distribuição local;
- simplifica testes.

### Credenciais

O Windows Credential Manager reduz risco de exposição de API keys. As chaves ficam fora do repositório, fora de arquivos de configuração e fora dos payloads de análise.

## Providers de IA

### OpenRouter

Vantagens:

- acesso a vários modelos por uma única API;
- possibilidade de usar famílias como Claude, Gemini, Llama, DeepSeek e Grok/xAI quando disponíveis na conta;
- facilita testes comparativos;
- reduz custo de manutenção de múltiplas integrações;
- permite alternar entre famílias de modelos rapidamente.

Limitações:

- disponibilidade varia conforme conta, créditos e rotas;
- alguns modelos podem retornar `404 No endpoints found`;
- custos e limites podem mudar conforme modelo.

### Providers Diretos

Providers diretos implementados:

- Gemini
- Groq
- OpenAI
- Anthropic
- Mistral
- Cohere

Vantagens:

- controle direto sobre API oficial;
- menor dependência de agregador;
- acesso a recursos específicos de cada fornecedor;
- possibilidade de deployments corporativos com outros provedores enterprise.

Limitações:

- cada provider possui formato, limite e política própria;
- exige mais manutenção;
- credenciais e billing precisam ser geridos separadamente.

## Benefícios da Arquitetura Multi-Provider

- Tolerância operacional: usuários podem trocar de provider manualmente.
- Comparação de qualidade entre modelos.
- Menor lock-in tecnológico.
- Evolução incremental com novos providers.
- Melhor adequação a custo, latência e disponibilidade.

## Custos

Custos dependem de:

- provider selecionado;
- modelo;
- tamanho da transcrição;
- `maxTokens`;
- temperatura e política de retry do provider;
- plano gratuito ou pago.

O projeto usa `maxTokens` configurável e recomenda padrão conservador de 1024 para reduzir surpresas de custo.

## Riscos Técnicos

- Modelos podem retornar JSON inválido ou incompleto.
- Quotas podem ser excedidas.
- Chaves podem expirar ou perder permissão.
- Catálogos de modelos mudam com frequência.
- Algumas APIs podem alterar contratos ou nomes de modelos.
- Dados sensíveis podem estar presentes nas transcrições.

## Mitigações

- Parser valida o schema esperado.
- Erros técnicos são preservados na interface.
- API keys ficam no Credential Manager.
- OpenRouter possui verificação de disponibilidade de modelos por chave.
- Providers ficam isolados em classes próprias.
- Testes cobrem serviços, controllers, parser e persistência.

## Limitações Atuais

- Não há login ou controle multiusuário.
- Não há transcrição automática de áudio.
- Não há dashboard de custos ou tokens.
- Não há sincronização em nuvem.
- Análise depende de provider externo disponível.

## Conclusão

O EasyMeet é viável para entrega acadêmica, portfólio e evolução incremental. A arquitetura escolhida equilibra simplicidade operacional com flexibilidade técnica, mantendo a IA como componente real do produto.
