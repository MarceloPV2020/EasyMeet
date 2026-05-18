# Backlog do Projeto EasyMeet

Este documento contém a lista de pendências e o roteiro de desenvolvimento do projeto EasyMeet.

## User Stories

### US01 - Documentação de Viabilidade
**Como** desenvolvedor/gestor do projeto,  
**Quero** ter um documento de viabilidade técnica e de negócio,  
**Para** justificar o uso de IA e documentar as limitações e o escopo do que foi implementado.

**Critérios de Aceitação:**
- Arquivo `docs/VIABILIDADE.md` criado e preenchido.
- Contém: problema identificado, justificativa do uso de IA, viabilidade técnica, o que foi implementado, o que funciona, limitações conhecidas, escopo implementado, proposta futura, custo/benefício, riscos e limitações técnicas.

**Cenário BDD:**
- **Dado que** o projeto está em fase de consolidação,
- **Quando** eu acessar a pasta `docs/`,
- **Então** devo encontrar o arquivo `VIABILIDADE.md` com todas as seções obrigatórias preenchidas.

**Status:** Pendente
**Prioridade:** Alta

---

### US02 - Modelagem Técnica e Diagramas UML
**Como** arquiteto de software,  
**Quero** visualizar a estrutura e o comportamento do sistema através de diagramas,  
**Para** facilitar o entendimento da arquitetura e a manutenção futura.

**Critérios de Aceitação:**
- Arquivo `docs/UML.md` criado usando Mermaid.
- Contém: diagrama de classes, diagrama de sequência, fluxograma da aplicação e arquitetura do sistema.
- Referencia os componentes: `ReunioesController`, `IAgenteResumoReuniaoService`, `AgenteResumoReuniaoService`, `PromptResumoReuniaoBuilder`, `GeminiClientService`, `ResumoReuniaoRequest`, `ResumoReuniaoResponse`.

**Cenário BDD:**
- **Dado que** preciso entender o fluxo de dados da aplicação,
- **Quando** eu ler o arquivo `docs/UML.md`,
- **Então** devo ver diagramas Mermaid renderizados corretamente com a lógica do sistema.

**Status:** Pendente
**Prioridade:** Alta

---

### US03 - Padronização de Contribuições (Pull Requests)
**Como** mantenedor do projeto,  
**Quero** que todas as alterações sejam submetidas via Pull Request com um template completo,  
**Para** garantir a qualidade do código e a rastreabilidade das mudanças.

**Critérios de Aceitação:**
- Pelo menos 1 Pull Request aberto seguindo o template definido em `.github/pull_request_template.md`.

**Cenário BDD:**
- **Dado que** uma nova funcionalidade foi finalizada,
- **Quando** eu abrir um Pull Request,
- **Então** as informações de descrição, testes e checklist devem estar preenchidas conforme o template.

**Status:** Pendente
**Prioridade:** Média

---

## Outras Pendências

| Tarefa | Descrição | Prioridade | Status |
|---|---|---|---|
| Criação de Slides | Elaborar apresentação do projeto (Slides) | Alta | Pendente |
| Melhorias de Código | Revisar e refatorar pontos de melhoria identificados no código | Média | Pendente |
| Revisão de Código | Realizar code review das implementações atuais | Média | Pendente |
