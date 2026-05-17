# DIRETRIZES DE IA - EasyMeet

## 1. Papel da IA no sistema
A IA no EasyMeet tem papel funcional e crítico no produto: transformar texto/transcrição de reuniões em saída estruturada para apoio à execução.

Objetivos principais:
- resumir reunião de forma clara e concisa;
- identificar tópicos principais;
- identificar ações práticas;
- identificar responsáveis;
- classificar o tipo de reunião;
- retornar nível de confiança da análise.

A IA não é apenas um recurso de apoio visual: ela é parte do fluxo principal da API.

## 2. Comportamento esperado do agente
O agente deve:
- priorizar precisão e objetividade;
- produzir resposta estruturada em JSON válido;
- usar linguagem neutra e profissional;
- reduzir inferências frágeis;
- sinalizar baixa confiança quando houver pouca evidência no texto;
- evitar qualquer conteúdo fora do escopo da reunião.

O agente não deve:
- inventar fatos, ações, responsáveis ou decisões não presentes no texto;
- retornar explicações fora do JSON solicitado;
- responder em markdown quando o contrato exige JSON puro.

## 3. Regras de Prompt Engineering
Regras obrigatórias de construção de prompt:
- definir explicitamente o formato de saída JSON esperado;
- proibir texto extra além do JSON;
- incluir instruções para intervalo de confiança (`0.0` a `1.0`);
- exigir arrays de string para listas (`topicosPrincipais`, `acoes`, `responsaveis`);
- orientar classificação para um conjunto controlado de tipos de reunião;
- fixar idioma de resposta em português (pt-BR) nesta versão;
- incluir a transcrição integral sem alteração semântica;
- reforçar política de não invenção (usar apenas evidências do texto).

Boas práticas:
- temperatura baixa para maior consistência estrutural;
- instruções curtas e inequívocas;
- schema explícito no prompt.

## 4. Formato obrigatório das respostas
A resposta do agente deve seguir obrigatoriamente:

```json
{
  "resumo": "",
  "topicosPrincipais": [],
  "acoes": [],
  "responsaveis": [],
  "tipoReuniao": "",
  "nivelConfianca": 0.0
}
```

Após processamento da API, o payload final ao cliente inclui:
- `geradoPorIA`;
- `modoExecucao` (`gemini` ou `fallback_local`).

## 5. Regras para respostas JSON
Regras técnicas:
- JSON deve ser válido e desserializável;
- todas as propriedades obrigatórias devem existir;
- arrays devem conter strings;
- `nivelConfianca` deve ser número entre `0.0` e `1.0`;
- `tipoReuniao` deve ser string não vazia (usar `Unknown` quando incerto);
- não incluir campos extras inesperados;
- não retornar blocos de código, markdown ou comentários.

## 6. Limitações da IA
Limitações conhecidas:
- pode falhar em textos ambíguos, incompletos ou ruidosos;
- pode confundir responsável quando a transcrição não explicita autoria;
- pode reduzir nuances contextuais em resumos muito curtos;
- pode retornar JSON malformado em situações adversas.

Consequências práticas:
- sempre validar estrutura e conteúdo antes de aceitar resposta;
- usar fallback local em qualquer violação do contrato.

## 7. Regras de fallback
Acionar fallback local quando ocorrer qualquer uma das condições:
- ausência de `GEMINI_API_KEY`;
- timeout na chamada ao provedor;
- erro HTTP do provedor;
- resposta sem conteúdo útil;
- JSON inválido ou fora do schema;
- falha de desserialização;
- campos obrigatórios ausentes/inválidos.

No fallback, a API deve retornar:
- `geradoPorIA = false`
- `modoExecucao = "fallback_local"`

## 8. Validações obrigatórias
### Entrada (request)
- rejeitar texto vazio;
- rejeitar texto muito curto (regra mínima configurada);
- validar texto informado.

### Saída da IA
- validar formato JSON;
- validar presença dos campos obrigatórios;
- validar tipos esperados;
- normalizar e limpar listas (trim, remoção de vazios, deduplicação);
- aplicar clamp do `nivelConfianca` para `[0,1]`.

## 9. Segurança
Diretrizes de segurança:
- nunca expor `GEMINI_API_KEY` em logs ou respostas;
- evitar logar transcrições completas em produção;
- sanitizar mensagens de erro retornadas ao cliente;
- tratar dados da reunião como potencialmente sensíveis;
- minimizar retenção de conteúdo bruto;
- usar timeouts e limites de tamanho quando possível.

## 10. Prevenção de respostas inválidas
Medidas preventivas:
- prompt com schema estrito e instrução de "somente JSON";
- `responseMimeType` configurado para `application/json`;
- parser robusto com extração defensiva de JSON;
- validação pós-parser de shape e tipos;
- fallback automático em qualquer inconsistência.

## 11. Tratamento de falhas
Fluxo recomendado:
1. Tentar chamada IA real.
2. Em falha técnica/estrutural, registrar log com contexto técnico (sem segredo).
3. Acionar fallback local.
4. Retornar resposta estável ao cliente (sem quebrar contrato).

Regras de observabilidade:
- logs de sucesso/falha por etapa;
- identificação clara de `modoExecucao`;
- monitoramento de taxa de fallback.

## 12. Diretrizes funcionais por tarefa
### 12.1 Resumir reuniões
- produzir resumo objetivo, curto e fiel ao texto;
- focar em decisões, bloqueios e próximos passos;
- evitar repetir detalhes irrelevantes.

### 12.2 Identificar ações
- extrair tarefas acionáveis, concretas e verificáveis;
- preferir verbo no infinitivo (ex: "Definir", "Enviar", "Validar").

### 12.3 Identificar responsáveis
- mapear responsáveis explicitamente citados;
- quando não houver evidência suficiente, usar indicação de indefinição sem inventar nomes.

### 12.4 Classificar reuniões
- classificar entre categorias conhecidas (ex: Daily, Planning, Status, Retrospective, OneOnOne, Incident, Executive, Unknown);
- usar `Unknown` quando sinais forem insuficientes ou conflitantes.

### 12.5 Evitar inventar informações
Regra central:
- toda informação retornada deve ser sustentada por evidência textual da transcrição.

Quando faltar evidência:
- reduzir assertividade;
- refletir incerteza no `nivelConfianca`;
- não criar fatos, prazos, pessoas ou decisões inexistentes.

## 13. Critério de qualidade mínima
Uma resposta só é considerada válida quando:
- respeita o JSON obrigatório;
- mantém aderência factual ao texto;
- apresenta consistência entre resumo, ações, responsáveis e tipo;
- não viola regras de segurança e fallback.

