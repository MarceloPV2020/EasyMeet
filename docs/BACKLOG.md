# Backlog - EasyMeet

Este backlog organiza o estado atual do EasyMeet e as próximas evoluções recomendadas para produto, IA, segurança e experiência do usuário.

## MVP Concluído

- API ASP.NET Core .NET 9.
- Swagger funcional.
- Interface web local.
- Configuração segura de API keys.
- Seleção dinâmica de provider de IA.
- Análise real com LLMs.
- Suporte a OpenRouter e providers diretos.
- Histórico local em SQLite.
- Visualização e filtros no histórico.
- Exportação PDF.
- Testes automatizados.
- GitHub Actions.

## Funcionalidades Implementadas

### Configuração de IA

- Selecionar provider para análise.
- Selecionar modelo, temperatura e máximo de tokens.
- Salvar padrões avançados por provider.
- Testar conexão com API key.
- Remover API key configurada.

### Análise de Reuniões

- Gerar resumo executivo.
- Identificar tópicos principais.
- Extrair ações com responsáveis e prazos quando disponíveis.
- Identificar decisões tomadas.
- Identificar pendências.
- Classificar tipo de reunião.
- Registrar nível de confiança.
- Registrar provider e modelo usados.

### Histórico e Exportação

- Persistir análises em SQLite.
- Listar histórico.
- Filtrar por texto, data e tipo de reunião.
- Remover itens do histórico.
- Exportar relatório em PDF.

## User Stories Implementadas

### US01 - Configurar Provider de IA

Como usuário, quero cadastrar e testar API keys por provider para usar modelos reais sem expor credenciais.

### US02 - Analisar Reunião com IA

Como usuário, quero colar uma transcrição e receber uma análise estruturada para acompanhar decisões e tarefas.

### US03 - Escolher Modelo

Como usuário, quero selecionar provider/modelo, temperatura e tokens para controlar qualidade, custo e previsibilidade.

### US04 - Consultar Histórico

Como usuário, quero visualizar análises anteriores para recuperar decisões, ações e pendências.

### US05 - Exportar PDF

Como usuário, quero exportar uma análise para PDF para compartilhar ou arquivar o resultado.

## Backlog Futuro

### Inteligência Artificial

- Comparação lado a lado entre providers.
- Ranking de qualidade por tipo de reunião.
- Métricas de custo por modelo.
- Registro de tokens usados quando o provider retornar essa informação.
- Sugestão de modelo conforme tamanho da transcrição.
- Avaliação automática de aderência ao JSON.

### UX/UI

- Tela detalhada para cada análise.
- Edição manual do resultado antes da exportação.
- Melhorar página de histórico para grandes volumes.
- Adicionar ordenação avançada por data, confiança e provider.
- Exibir indicadores de custo e latência por análise.

### Integrações

- Upload de áudio.
- Transcrição automática.
- Importação de `.txt`, `.docx` e `.pdf`.
- Exportação em DOCX e Markdown.
- Envio de resumo por e-mail.
- Integração futura com calendário.

### Segurança

- Autenticação local ou multiusuário.
- Perfis e permissões.
- Criptografia adicional do histórico.
- Mascaramento opcional de dados sensíveis antes do envio ao LLM.
- Política de retenção de histórico.

### Analytics

- Dashboard de reuniões analisadas.
- Tempo médio de resposta por provider.
- Taxa de erro por modelo.
- Custo estimado por análise.
- Distribuição por tipo de reunião.

### Colaboração

- Comentários em análises.
- Compartilhamento de relatórios.
- Edição manual de ações e responsáveis.
- Status de execução das ações.
- Atribuição de responsáveis internos.

## Priorização Sugerida

- P1: transcrição automática de áudio.
- P1: dashboard básico de histórico.
- P1: métricas de provider/modelo.
- P2: exportação DOCX/Markdown.
- P2: filtros avançados por provider/modelo.
- P2: login e perfis.
- P3: colaboração e workflow de ações.
