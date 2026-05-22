# Backlog - EasyMeet

Este backlog organiza o estado atual do EasyMeet e as próximas evoluções recomendadas para produto, IA, segurança, experiência do usuário e rastreabilidade no GitHub.

## MVP Concluído

- [x] API ASP.NET Core .NET 9.
- [x] Swagger funcional.
- [x] Interface web local.
- [x] Configuração segura de API keys.
- [x] Seleção dinâmica de provider de IA.
- [x] Análise real com LLMs.
- [x] Suporte a OpenRouter e providers diretos.
- [x] Histórico local persistido em SQLite.
- [x] Visualização e filtros no histórico.
- [x] Exportação PDF.
- [x] Testes automatizados.
- [x] GitHub Actions.
- [x] Pull Request template.
- [x] Arquitetura organizada em controllers, services, providers, models e prompts.

## Funcionalidades Implementadas

### IA e Providers

- [x] OpenRouter integrado.
- [x] Gemini integrado.
- [x] Groq integrado.
- [x] OpenAI integrado.
- [x] Anthropic integrado.
- [x] Mistral integrado.
- [x] Cohere integrado.
- [x] Selecionar provider para análise.
- [x] Selecionar modelo, temperatura e máximo de tokens.
- [x] Salvar padrões avançados por provider.
- [x] Testar conexão com API key.
- [x] Remover API key configurada.
- [x] Filtrar modelos OpenRouter por disponibilidade da chave configurada.

### Análise de Reuniões

- [x] Gerar resumo executivo.
- [x] Identificar tópicos principais.
- [x] Extrair ações com responsáveis e prazos quando disponíveis.
- [x] Identificar responsáveis.
- [x] Identificar decisões tomadas.
- [x] Identificar pendências.
- [x] Classificar tipo de reunião.
- [x] Registrar nível de confiança.
- [x] Registrar provider e modelo usados no histórico.
- [x] Validar JSON retornado por LLMs.
- [x] Executar exatamente o modelo selecionado, sem fallback automático.

### Histórico, Exportação e Interface

- [x] Persistir análises em SQLite.
- [x] Listar histórico.
- [x] Filtrar histórico por texto, data e tipo de reunião.
- [x] Remover itens do histórico.
- [x] Exportar relatório em PDF.
- [x] Exibir interface web responsiva para análise e histórico.

### Qualidade e Entrega

- [x] Swagger disponível para exploração da API.
- [x] Testes automatizados xUnit.
- [x] Testes de controllers, services, parser, repositório SQLite e pipeline HTTP.
- [x] GitHub Actions para restore, build, testes e cobertura mínima.
- [x] Template de Pull Request.
- [x] Documentação técnica e acadêmica.

## User Stories Implementadas

### US01 - Configurar Provider de IA

- [x] Como usuário, quero cadastrar e testar API keys por provider para usar modelos reais sem expor credenciais.

### US02 - Analisar Reunião com IA

- [x] Como usuário, quero colar uma transcrição e receber uma análise estruturada para acompanhar decisões e tarefas.

### US03 - Escolher Modelo

- [x] Como usuário, quero selecionar provider/modelo, temperatura e tokens para controlar qualidade, custo e previsibilidade.

### US04 - Consultar Histórico

- [x] Como usuário, quero visualizar análises anteriores para recuperar decisões, ações e pendências.

### US05 - Exportar PDF

- [x] Como usuário, quero exportar uma análise para PDF para compartilhar ou arquivar o resultado.

## Melhorias Futuras

### IA

- [ ] Comparação lado a lado entre providers.
- [ ] Ranking de qualidade por tipo de reunião.
- [ ] Registro de tokens usados quando o provider retornar essa informação.
- [ ] Sugestão de modelo conforme tamanho da transcrição.
- [ ] Avaliação automática de aderência ao JSON.

### UX/UI

- [ ] Tela detalhada para cada análise.
- [ ] Edição manual do resultado antes da exportação.
- [ ] Melhorar página de histórico para grandes volumes.
- [ ] Adicionar ordenação avançada por data, confiança e provider.
- [ ] Exibir indicadores de custo e latência por análise.

### Infraestrutura

- [ ] Criar release workflow.
- [ ] Publicar artefatos de cobertura no GitHub Actions.
- [ ] Adicionar configuração por ambiente para produção.
- [ ] Criar rotina de backup/exportação do banco SQLite.

### Analytics

- [ ] Dashboard de reuniões analisadas.
- [ ] Tempo médio de resposta por provider.
- [ ] Taxa de erro por modelo.
- [ ] Custo estimado por análise.
- [ ] Distribuição por tipo de reunião.

### Segurança

- [ ] Autenticação local ou multiusuário.
- [ ] Perfis e permissões.
- [ ] Criptografia adicional do histórico.
- [ ] Mascaramento opcional de dados sensíveis antes do envio ao LLM.
- [ ] Política de retenção de histórico.

### Colaboração

- [ ] Comentários em análises.
- [ ] Compartilhamento de relatórios.
- [ ] Edição colaborativa de ações e responsáveis.
- [ ] Status de execução das ações.
- [ ] Atribuição de responsáveis internos.

### Providers

- [ ] Comparar custo, latência e qualidade entre providers.
- [ ] Permitir cadastro customizado de novos modelos OpenRouter.
- [ ] Exibir disponibilidade de modelos por conta/chave.
- [ ] Registrar erros por provider para diagnóstico.

### Exportação

- [ ] Exportação em DOCX.
- [ ] Exportação em Markdown.
- [ ] Envio de resumo por e-mail.
- [ ] Template customizável para relatórios.

### Áudio e Transcrição

- [ ] Upload de áudio.
- [ ] Transcrição automática.
- [ ] Importação de `.txt`, `.docx` e `.pdf`.
- [ ] Integração futura com calendário.

## Priorização Sugerida

- [ ] Alta: transcrição automática de áudio.
- [ ] Alta: dashboard básico de histórico.
- [ ] Alta: métricas de provider/modelo.
- [ ] Média: exportação DOCX/Markdown.
- [ ] Média: filtros avançados por provider/modelo.
- [ ] Média: login e perfis.
- [ ] Baixa: colaboração e workflow de ações.
