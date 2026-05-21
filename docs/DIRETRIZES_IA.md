# Diretrizes de IA - EasyMeet

## Objetivo

Definir regras de comportamento, qualidade, segurança e operação para o uso de IA no EasyMeet.

## Papel da IA

A IA é um componente funcional do produto. Ela não é apenas uma ferramenta auxiliar de desenvolvimento: o sistema depende dela para interpretar transcrições e gerar informações estruturadas.

A IA deve:

- resumir a reunião;
- identificar tópicos principais;
- extrair ações;
- identificar responsáveis;
- registrar decisões tomadas;
- apontar pendências;
- classificar o tipo de reunião;
- estimar nível de confiança.

## Providers

O EasyMeet usa arquitetura multi-provider.

Providers implementados:

- OpenRouter
- Gemini
- Groq
- OpenAI
- Anthropic
- Mistral
- Cohere

OpenRouter permite acessar vários modelos por uma única API. Providers diretos permanecem disponíveis para controle oficial, comparação técnica e redução de dependência de agregadores.

## Seleção Dinâmica

O usuário escolhe o provider antes da análise. Opcionalmente também escolhe:

- modelo;
- temperatura;
- máximo de tokens.

Quando o usuário informa um modelo, o sistema executa exatamente aquele modelo. Quando não informa, o provider usa seu padrão configurado.

## Sem Fallback Automático

O EasyMeet não troca automaticamente de provider ou modelo quando ocorre erro. Essa decisão preserva previsibilidade e rastreabilidade.

Se um modelo falhar, a interface informa o erro e o usuário pode selecionar outro modelo manualmente.

## Prompt Engineering

O prompt deve:

- instruir a IA a retornar somente JSON;
- reforçar que informações ausentes não devem ser inventadas;
- exigir campos obrigatórios;
- orientar formato de ações, responsáveis, decisões e pendências;
- usar linguagem objetiva e profissional;
- evitar markdown, explicações extras ou texto fora do JSON.

## Schema Esperado

```json
{
  "resumo": "",
  "topicosPrincipais": [],
  "acoes": [
    {
      "descricao": "",
      "responsavel": "",
      "prazo": ""
    }
  ],
  "responsaveis": [],
  "decisoesTomadas": [],
  "pendencias": [],
  "dataReuniao": "",
  "tipoReuniao": "",
  "nivelConfianca": 0.0
}
```

O parser também aceita `decisoes` como alternativa legada a `decisoesTomadas`.

## Regras de Qualidade

- Resumo deve ser fiel ao conteúdo da transcrição.
- Tópicos devem representar temas realmente discutidos.
- Ações devem ser objetivas e executáveis.
- Responsáveis devem ser indicados apenas quando houver evidência.
- Prazos não devem ser inventados.
- Decisões devem ser separadas de pendências.
- Nível de confiança deve ficar entre `0.0` e `1.0`.

## Diferenças Entre Modelos

Embora todos usem o mesmo contrato da aplicação, modelos podem variar em:

- aderência ao JSON;
- qualidade do resumo;
- capacidade de extrair ações;
- limite de contexto;
- custo;
- latência;
- disponibilidade por chave;
- sensibilidade a temperatura.

## Temperatura

Temperatura baixa é recomendada para análise de reuniões, pois favorece consistência e reduz invenções. O padrão recomendado é `0.2`.

## MaxTokens

`maxTokens` controla o tamanho máximo da resposta. Valores maiores podem ajudar em reuniões longas, mas também aumentam custo e risco de exceder limites do provider. O padrão recomendado é `1024`.

## Contexto

O modelo deve analisar apenas o texto recebido na transcrição. Informações externas, suposições e inferências sem base devem ser evitadas.

## Privacidade e Segurança

- API keys não devem ser registradas em logs.
- API keys não devem ser salvas em arquivos do repositório.
- API keys não devem trafegar no payload de análise.
- Transcrições podem conter dados sensíveis e devem ser tratadas com cuidado.
- O usuário deve revisar o resultado antes de usar como documento oficial.

## Tratamento de Erros

Erros comuns:

- chave ausente;
- chave inválida;
- quota excedida;
- limite de requisições;
- modelo indisponível;
- modelo descontinuado;
- JSON inválido retornado pelo modelo;
- timeout;
- instabilidade temporária do provider.

A interface deve exibir mensagem amigável e preservar detalhe técnico útil quando disponível.

## Uso Responsável

O EasyMeet deve apoiar o usuário, não substituir revisão humana. Resultados de IA podem conter erros, omissões ou interpretações imperfeitas. Decisões importantes devem ser validadas por uma pessoa.

## Recomendações Operacionais

- Usar modelos mais consistentes para atas e decisões.
- Preferir temperatura baixa.
- Reduzir `maxTokens` em testes de custo.
- Trocar manualmente de modelo quando houver erro de disponibilidade.
- Evitar enviar dados sigilosos para providers externos sem política adequada.
