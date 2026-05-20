# Diretrizes de IA - EasyMeet

## Objetivo
Definir regras de uso, segurança e qualidade para as respostas geradas por IA no EasyMeet.

## Princípios
- Toda análise válida deve usar IA real.
- Não existe fallback local para gerar resumo, ações ou decisões.
- A IA deve trabalhar apenas com informações presentes na transcrição.
- O resultado deve ser objetivo, corporativo e diretamente utilizável.
- A resposta precisa ser JSON válido e aderente ao schema esperado.

## Schema esperado
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
  "tipoReuniao": "",
  "nivelConfianca": 0.0
}
```

## Compatibilidade mantida
- O parser ainda aceita `decisoes` como alternativa legada a `decisoesTomadas`.
- O parser ainda normaliza `acoes` em formato de texto simples, quando recebido de provedores menos aderentes ao prompt.

## Regras de geração
- Retornar somente JSON, sem markdown e sem texto adicional.
- Consolidar ações duplicadas.
- Destacar decisões tomadas de forma objetiva.
- Identificar responsáveis quando houver evidência na transcrição.
- Marcar pendências sem inventar prazos ou responsáveis.
- Classificar o tipo de reunião com linguagem clara.
- Informar `nivelConfianca` entre `0.0` e `1.0`.

## Configurações de inferência
- `modelo`: define o modelo/deployment usado no provedor selecionado.
- `temperatura`: controla variação da resposta.
- `maxTokens`: limita o tamanho máximo da resposta.
- Quando uma configuração não é informada, o provedor usa o padrão definido no backend.
- A aplicação não troca automaticamente de modelo quando ocorre erro.

## Segurança
- Nunca registrar API key em log.
- Nunca retornar API key em resposta de endpoint.
- Nunca persistir API key em arquivos de configuração, arquivos temporários ou repositório.
- Usar Windows Credential Manager como mecanismo oficial de armazenamento local.

## Tratamento de erros
- Exibir mensagem amigável para o usuário final.
- Preservar detalhe técnico quando o provedor retornar uma mensagem útil.
- Tratar explicitamente casos como chave expirada, quota excedida, limite de requisições, modelo inválido, modelo descontinuado e indisponibilidade temporária.
