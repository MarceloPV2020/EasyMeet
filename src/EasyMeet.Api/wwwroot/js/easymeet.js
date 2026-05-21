const toastEl = document.getElementById("toast");
    const tabAnaliseBtn = document.getElementById("tabAnalise");
    const tabHistoricoBtn = document.getElementById("tabHistorico");
    const analiseViewEl = document.getElementById("analiseView");
    const historicoViewEl = document.getElementById("historicoView");
    const historicoStateEl = document.getElementById("historicoState");
    const historicoListEl = document.getElementById("historicoList");
    const refreshHistoricoBtn = document.getElementById("refreshHistorico");
    const historicoFiltroTextoEl = document.getElementById("historicoFiltroTexto");
    const historicoFiltroDataEl = document.getElementById("historicoFiltroData");
    const historicoFiltroTipoEl = document.getElementById("historicoFiltroTipo");
    const limparFiltrosHistoricoBtn = document.getElementById("limparFiltrosHistorico");
    const historicoPaginaAnteriorBtn = document.getElementById("historicoPaginaAnterior");
    const historicoProximaPaginaBtn = document.getElementById("historicoProximaPagina");
    const historicoPaginaInfoEl = document.getElementById("historicoPaginaInfo");
    const toggleConfigBtn = document.getElementById("toggleConfig");
    const configDrawerEl = document.getElementById("configDrawer");
    const configBackdropEl = document.getElementById("configBackdrop");
    const providerConfigEl = document.getElementById("providerConfig");
    const providerAnalysisEl = document.getElementById("providerAnalysis");
    const sidebarProvidersEl = document.getElementById("sidebarProviders");
    const providerStatusEl = document.getElementById("providerStatus");
    const apiKeyEl = document.getElementById("apiKey");
    const transcricaoEl = document.getElementById("transcricao");
    const switchToGroqBtn = document.getElementById("switchToGroq");
    const modeloIaEl = document.getElementById("modeloIa");
    const temperaturaIaEl = document.getElementById("temperaturaIa");
    const maxTokensIaEl = document.getElementById("maxTokensIa");
    const saveDefaultsIaBtn = document.getElementById("saveDefaultsIa");
    const clearDefaultsIaBtn = document.getElementById("clearDefaultsIa");
    const analysisConfigSummaryEl = document.getElementById("analysisConfigSummary");

    const resumoEl = document.getElementById("resumo");
    const topicosEl = document.getElementById("topicos");
    const acoesEl = document.getElementById("acoes");
    const responsaveisEl = document.getElementById("responsaveis");
    const decisoesEl = document.getElementById("decisoes");
    const pendenciasEl = document.getElementById("pendencias");
    const dataReuniaoEl = document.getElementById("dataReuniao");
    const tipoReuniaoEl = document.getElementById("tipoReuniao");
    const confiancaEl = document.getElementById("confianca");

    const saveKeyBtn = document.getElementById("saveKey");
    const testKeyBtn = document.getElementById("testKey");
    const deleteKeyBtn = document.getElementById("deleteKey");
    const analisarBtn = document.getElementById("analisar");
    const advancedDefaultsStorageKey = "easymeet:ia-advanced-defaults:v1";
    const lastProviderStorageKey = "easymeet:last-provider:v1";
    const modelosPorProvedor = {
      Gemini: [
        { value: "", label: "Padrão do provedor (gemini-2.5-flash)" },
        { value: "gemini-2.5-flash", label: "gemini-2.5-flash" },
        { value: "gemini-2.5-flash-lite", label: "gemini-2.5-flash-lite" },
        { value: "gemini-2.0-flash", label: "gemini-2.0-flash" },
        { value: "gemini-1.5-flash", label: "gemini-1.5-flash" }
      ],
      Groq: [
        { value: "", label: "Padrão do provedor (llama-3.1-8b-instant)" },
        { value: "llama-3.1-8b-instant", label: "llama-3.1-8b-instant" },
        { value: "llama-3.3-70b-versatile", label: "llama-3.3-70b-versatile" }
      ],
      OpenAI: [
        { value: "", label: "Padrão do provedor (gpt-4.1-mini)" },
        { value: "gpt-4.1-mini", label: "gpt-4.1-mini" },
        { value: "gpt-4.1", label: "gpt-4.1" }
      ],
      Anthropic: [
        { value: "", label: "Padrão do provedor (claude-3-7-sonnet-20250219)" },
        { value: "claude-3-7-sonnet-20250219", label: "claude-3-7-sonnet-20250219" },
        { value: "claude-sonnet-4-20250514", label: "claude-sonnet-4-20250514" }
      ],
      Mistral: [
        { value: "", label: "Padrão do provedor (mistral-small-latest)" },
        { value: "mistral-small-latest", label: "mistral-small-latest" },
        { value: "mistral-medium-latest", label: "mistral-medium-latest" }
      ],
      Cohere: [
        { value: "", label: "Padrão do provedor (command-a-03-2025)" },
        { value: "command-a-03-2025", label: "command-a-03-2025" },
        { value: "command-r-08-2024", label: "command-r-08-2024" }
      ],
      AzureOpenAI: [
        { value: "", label: "Padrão do provedor (deployment configurado)" },
        { value: "gpt-4.1-mini", label: "gpt-4.1-mini (deployment)" },
        { value: "gpt-4.1", label: "gpt-4.1 (deployment)" }
      ]
    };
    const maxTokensSugeridoPorProvedor = {
      Gemini: 1024,
      Groq: 1024,
      OpenAI: 1024,
      Anthropic: 1024,
      Mistral: 1024,
      Cohere: 1024,
      AzureOpenAI: 1024
    };
    const temperaturaPadraoPorProvedor = {
      Gemini: 0.2,
      Groq: 0.2,
      OpenAI: 0.2,
      Anthropic: 0.2,
      Mistral: 0.2,
      Cohere: 0.2,
      AzureOpenAI: 0.2
    };

    let providerStatus = {};
    let providersCatalog = [];
    let toastTimeout;
    let historicoCarregado = false;
    let historicoOriginal = [];
    let historicoPaginaAtual = 1;
    const historicoItensPorPagina = 6;
    let analysisStatus = { text: "Pronto", className: "chip info" };

    function syncDrawerState(open) {
      configDrawerEl.classList.toggle("open", open);
      configBackdropEl.classList.toggle("open", open);
      configDrawerEl.setAttribute("aria-hidden", open ? "false" : "true");
      toggleConfigBtn.innerHTML = open
        ? '<span class="nav-icon" aria-hidden="true">×</span>Ocultar configuração'
        : '<span class="nav-icon" aria-hidden="true">⚙</span>Configuração IA';
      toggleConfigBtn.setAttribute("aria-expanded", String(open));
      document.body.style.overflow = open ? "hidden" : "";
    }

    function toggleConfigDrawer() {
      const open = !configDrawerEl.classList.contains("open");
      syncDrawerState(open);
    }

    function closeConfigDrawer() {
      syncDrawerState(false);
    }

    function showToast(message, isError = false) {
      toastEl.style.display = "block";
      toastEl.textContent = message;
      toastEl.style.background = isError ? "#b91c1c" : "#1f2937";
      if (toastTimeout) clearTimeout(toastTimeout);
      toastTimeout = setTimeout(() => toastEl.style.display = "none", 4500);
    }

    function setActiveTab(tab) {
      const isHistorico = tab === "historico";
      analiseViewEl.hidden = isHistorico;
      historicoViewEl.hidden = !isHistorico;
      tabAnaliseBtn.classList.toggle("active", !isHistorico);
      tabHistoricoBtn.classList.toggle("active", isHistorico);

      if (isHistorico && !historicoCarregado) {
        fetchHistorico();
      }
    }

    function formatConfianca(value) {
      const number = Number(value);
      return Number.isFinite(number)
        ? `${Math.round(number * 100)}%`
        : "Não informado";
    }

    function formatDataReuniao(value) {
      if (!value) {
        return "Não identificada";
      }

      const parts = value.toString().split("-");
      return parts.length === 3
        ? `${parts[2]}/${parts[1]}/${parts[0]}`
        : value;
    }

    function createHistorySection(title, content) {
      const section = document.createElement("section");
      section.className = "history-section";

      const heading = document.createElement("h4");
      heading.textContent = title;

      section.appendChild(heading);

      if (content instanceof Node) {
        section.appendChild(content);
      } else {
        const text = document.createElement("p");
        text.className = "value";
        text.textContent = content || "Não identificado";
        section.appendChild(text);
      }

      return section;
    }

    function createHistoryList(items) {
      const list = document.createElement("ul");
      if (!Array.isArray(items) || items.length === 0) {
        const empty = document.createElement("li");
        empty.className = "muted";
        empty.textContent = "Não identificado";
        list.appendChild(empty);
        return list;
      }

      items.forEach(item => {
        const li = document.createElement("li");
        li.textContent = item;
        list.appendChild(li);
      });
      return list;
    }

    function createHistoryActions(actions) {
      const list = document.createElement("ul");
      list.className = "actions-list";
      if (!Array.isArray(actions) || actions.length === 0) {
        const empty = document.createElement("li");
        empty.className = "muted";
        empty.textContent = "Não identificado";
        list.appendChild(empty);
        return list;
      }

      actions.forEach(item => {
        const li = document.createElement("li");
        li.className = "action-item";
        const descricao = item?.descricao || "Sem descrição";
        const responsavel = item?.responsavel || "Não identificado";
        const prazo = item?.prazo || "";
        const meta = prazo ? `Responsável: ${responsavel} | Prazo: ${prazo}` : `Responsável: ${responsavel}`;
        li.textContent = descricao;

        const metaEl = document.createElement("div");
        metaEl.className = "action-meta";
        metaEl.textContent = meta;
        li.appendChild(metaEl);
        list.appendChild(li);
      });

      return list;
    }

    function normalizeText(value) {
      return (value || "").toString().toLocaleLowerCase("pt-BR").normalize("NFD").replace(/[\u0300-\u036f]/g, "");
    }

    function uniqueValues(items, selector) {
      return [...new Set(items.map(selector).filter(value => value && value.toString().trim()))]
        .sort((a, b) => a.localeCompare(b, "pt-BR"));
    }

    function populateHistoryFilterOptions(reunioes) {
      const selectedTipo = historicoFiltroTipoEl.value;

      historicoFiltroTipoEl.innerHTML = '<option value="">Todos</option>';
      uniqueValues(reunioes, item => item.tipoReuniao).forEach(value => {
        const option = document.createElement("option");
        option.value = value;
        option.textContent = value;
        historicoFiltroTipoEl.appendChild(option);
      });

      if (Array.from(historicoFiltroTipoEl.options).some(option => option.value === selectedTipo)) {
        historicoFiltroTipoEl.value = selectedTipo;
      }
    }

    function buildHistorySearchText(reuniao) {
      const acoes = Array.isArray(reuniao.acoes)
        ? reuniao.acoes.map(acao => `${acao?.descricao || ""} ${acao?.responsavel || ""} ${acao?.prazo || ""}`).join(" ")
        : "";

      return normalizeText([
        reuniao.resumo,
        reuniao.transcricao,
        reuniao.dataReuniao,
        reuniao.tipoReuniao,
        reuniao.modoExecucao,
        ...(reuniao.topicosPrincipais || []),
        ...(reuniao.responsaveis || []),
        ...(reuniao.decisoes || []),
        ...(reuniao.pendencias || []),
        acoes
      ].join(" "));
    }

    function filtrarHistorico() {
      const termo = normalizeText(historicoFiltroTextoEl.value);
      const data = historicoFiltroDataEl.value;
      const tipo = historicoFiltroTipoEl.value;

      return historicoOriginal.filter(reuniao => {
        const matchesText = !termo || buildHistorySearchText(reuniao).includes(termo);
        const matchesData = !data || reuniao.dataReuniao === data;
        const matchesTipo = !tipo || reuniao.tipoReuniao === tipo;

        return matchesText && matchesData && matchesTipo;
      });
    }

    function getHistorySortValue(reuniao) {
      if (reuniao.dataReuniao) {
        const time = Date.parse(`${reuniao.dataReuniao}T00:00:00`);
        if (Number.isFinite(time)) {
          return time;
        }
      }

      return 0;
    }

    function ordenarHistoricoPorMaisRecente(reunioes) {
      return [...reunioes].sort((a, b) => {
        const dateDiff = getHistorySortValue(b) - getHistorySortValue(a);
        if (dateDiff !== 0) {
          return dateDiff;
        }

        return Number(b.id || 0) - Number(a.id || 0);
      });
    }

    function updateHistoricoPagination(totalItems, totalPages) {
      historicoPaginaInfoEl.textContent = `Página ${historicoPaginaAtual} de ${totalPages}`;
      historicoPaginaAnteriorBtn.disabled = historicoPaginaAtual <= 1;
      historicoProximaPaginaBtn.disabled = historicoPaginaAtual >= totalPages;
      const shouldShow = totalItems > historicoItensPorPagina;
      historicoPaginaAnteriorBtn.style.display = shouldShow ? "" : "none";
      historicoProximaPaginaBtn.style.display = shouldShow ? "" : "none";
      historicoPaginaInfoEl.style.display = shouldShow ? "" : "none";
    }

    function aplicarFiltrosHistorico() {
      const filtradas = ordenarHistoricoPorMaisRecente(filtrarHistorico());
      const totalPages = Math.max(1, Math.ceil(filtradas.length / historicoItensPorPagina));
      historicoPaginaAtual = Math.min(historicoPaginaAtual, totalPages);
      const start = (historicoPaginaAtual - 1) * historicoItensPorPagina;
      const pagina = filtradas.slice(start, start + historicoItensPorPagina);
      renderHistorico(pagina);
      updateHistoricoPagination(filtradas.length, totalPages);
      historicoStateEl.textContent = `${filtradas.length} de ${historicoOriginal.length} registros`;
      historicoStateEl.className = "chip ok";
    }

    function limparFiltrosHistorico() {
      historicoFiltroTextoEl.value = "";
      historicoFiltroDataEl.value = "";
      historicoFiltroTipoEl.value = "";
      historicoPaginaAtual = 1;
      aplicarFiltrosHistorico();
    }

    function renderHistorico(reunioes) {
      historicoListEl.innerHTML = "";

      if (!Array.isArray(reunioes) || reunioes.length === 0) {
        const empty = document.createElement("p");
        empty.className = "muted";
        empty.textContent = "Nenhuma reunião salva ainda.";
        historicoListEl.appendChild(empty);
        return;
      }

      reunioes.forEach(reuniao => {
        const item = document.createElement("article");
        item.className = "history-item";

        const head = document.createElement("div");
        head.className = "history-head";

        const title = document.createElement("span");
        title.className = "history-title";
        title.textContent = `Reunião #${reuniao.id}`;

        const confidence = document.createElement("span");
        confidence.className = "history-confidence";
        confidence.textContent = `Confiança ${formatConfianca(reuniao.confianca)}`;

        const pdfButton = document.createElement("button");
        pdfButton.type = "button";
        pdfButton.className = "history-pdf";
        pdfButton.textContent = "Gerar PDF";
        pdfButton.title = "Gerar PDF deste registro";
        pdfButton.addEventListener("click", () => gerarPdfAnalise(reuniao));

        const removeButton = document.createElement("button");
        removeButton.type = "button";
        removeButton.className = "history-remove";
        removeButton.textContent = "Excluir registro";
        removeButton.title = "Excluir este registro do histórico";
        removeButton.addEventListener("click", () => removerHistorico(reuniao.id));

        const historyActions = document.createElement("div");
        historyActions.className = "history-actions";
        historyActions.append(pdfButton, removeButton);

        const summary = document.createElement("p");
        summary.className = "history-summary";
        summary.textContent = reuniao.resumo || "Resumo não informado";

        const meta = document.createElement("div");
        meta.className = "history-meta";

        const tipoChip = document.createElement("span");
        tipoChip.className = "chip info";
        tipoChip.textContent = reuniao.tipoReuniao || "Tipo não identificado";

        const dataChip = document.createElement("span");
        dataChip.className = "chip info";
        dataChip.textContent = `Data: ${formatDataReuniao(reuniao.dataReuniao)}`;

        const iaChip = document.createElement("span");
        iaChip.className = "chip info";
        iaChip.textContent = reuniao.modoExecucao || "IA não identificada";

        const origemChip = document.createElement("span");
        origemChip.className = reuniao.geradoPorIA ? "chip ok" : "chip bad";
        origemChip.textContent = reuniao.geradoPorIA ? "Gerado por IA" : "Origem não identificada";

        meta.append(dataChip, tipoChip, iaChip, origemChip);

        const analysisDetails = document.createElement("details");
        const analysisSummary = document.createElement("summary");
        analysisSummary.textContent = "Ver análise completa";

        const detailsGrid = document.createElement("div");
        detailsGrid.className = "history-details-grid";
        detailsGrid.append(
          createHistorySection("Tópicos", createHistoryList(reuniao.topicosPrincipais)),
          createHistorySection("Ações", createHistoryActions(reuniao.acoes)),
          createHistorySection("Responsáveis", createHistoryList(reuniao.responsaveis)),
          createHistorySection("Decisões", createHistoryList(reuniao.decisoes)),
          createHistorySection("Pendências", createHistoryList(reuniao.pendencias))
        );
        analysisDetails.append(analysisSummary, detailsGrid);

        const transcriptionDetails = document.createElement("details");
        const detailsSummary = document.createElement("summary");
        detailsSummary.textContent = "Ver transcrição";

        const transcription = document.createElement("p");
        transcription.className = "history-transcription";
        transcription.textContent = reuniao.transcricao || "Transcrição não informada";

        head.append(title, confidence, historyActions);
        transcriptionDetails.append(detailsSummary, transcription);
        item.append(head, summary, meta, analysisDetails, transcriptionDetails);
        historicoListEl.appendChild(item);
      });
    }

    async function removerHistorico(id) {
      if (!id) {
        showToast("Registro inválido para remoção.", true);
        return;
      }

      const confirmado = window.confirm("Excluir este registro do histórico?");
      if (!confirmado) {
        return;
      }

      try {
        const resp = await fetch(`/api/reunioes/${id}`, { method: "DELETE" });
        if (!resp.ok && resp.status !== 204) {
          let message = "Não foi possível remover o registro.";
          try {
            const payload = await resp.json();
            message = payload.mensagem || payload.detail || message;
          } catch {}

          throw new Error(message);
        }

        historicoOriginal = historicoOriginal.filter(reuniao => reuniao.id !== id);
        aplicarFiltrosHistorico();
        showToast("Registro excluído do histórico.");
      } catch (err) {
        showToast(err.message || "Falha ao remover registro do histórico.", true);
      }
    }

    async function fetchHistorico(showLoading = true) {
      if (showLoading) {
        historicoStateEl.textContent = "Carregando";
        historicoStateEl.className = "chip info";
      }

      try {
        const resp = await fetch("/reunioes");
        if (!resp.ok) {
          throw new Error("Falha ao carregar histórico.");
        }

        const reunioes = await resp.json();
        historicoOriginal = ordenarHistoricoPorMaisRecente(Array.isArray(reunioes) ? reunioes : []);
        historicoPaginaAtual = 1;
        populateHistoryFilterOptions(historicoOriginal);
        aplicarFiltrosHistorico();
        historicoCarregado = true;
      } catch (err) {
        historicoStateEl.textContent = "Erro";
        historicoStateEl.className = "chip bad";
        historicoCarregado = false;
        historicoOriginal = [];
        historicoListEl.innerHTML = "";
        const error = document.createElement("p");
        error.className = "muted";
        error.textContent = err.message || "Não foi possível carregar o histórico.";
        historicoListEl.appendChild(error);
      }
    }

    function mapFriendlyErrorMessage(rawMessage) {
      const message = (rawMessage || "").toString().trim();
      const lower = message.toLowerCase();

      if (!message) {
        return "Não foi possível concluir a análise agora. Tente novamente em instantes.";
      }

      if (lower.includes("chave de api nao configurada") || lower.includes("chave de api não configurada")) {
        return "A chave de API do provedor selecionado ainda não foi configurada. Abra 'Configurar IA' e salve a chave.";
      }

      if (lower.includes("chave de api nao informada") || lower.includes("chave de api não informada")) {
        return "A chave de API informada está ausente. Verifique a configuração do provedor.";
      }

      if (lower.includes("status 401") || lower.includes("status 403")) {
        return "Não foi possível autenticar no provedor de IA. Confira a API Key e tente novamente.";
      }

      if (lower.includes("status 429") || lower.includes("too many requests")) {
        if (lower.includes("insufficient_quota") || lower.includes("quota") || lower.includes("billing") || lower.includes("credit")) {
          return "Sua conta/projeto atingiu o limite de uso (quota/crédito) no provedor. Verifique billing, orçamento e limites da API.";
        }
        return "O provedor recebeu muitas requisições agora. Aguarde alguns segundos e tente novamente.";
      }

      if (lower.includes("status 503") || lower.includes("high demand") || lower.includes("service unavailable")) {
        return "O provedor de IA está com alta demanda no momento. Tente novamente em instantes.";
      }

      if (lower.includes("status 504") || lower.includes("gateway timeout") || lower.includes("tempo limite")) {
        return "A resposta da IA demorou mais do que o esperado. Tente novamente.";
      }

      if (lower.includes("decommissioned") || lower.includes("no longer supported")) {
        return "O modelo selecionado foi descontinuado pelo provedor. Escolha outro modelo e tente novamente.";
      }

      if (lower.includes("status 404") && lower.includes("modelo")) {
        return "O modelo selecionado não está disponível para sua conta/chave. Selecione outro modelo.";
      }

      if (lower.includes("status 400")) {
        return "Não foi possível processar a solicitação com os parâmetros atuais. Revise o modelo/configurações e tente novamente.";
      }

      return message;
    }

    function shouldSuggestGroqSwitch(rawMessage) {
      const msg = (rawMessage || "").toString().toLowerCase();
      return msg.includes("gemini retornou status 429")
        && (msg.includes("exceeded your current quota") || msg.includes("quota"));
    }

    function hideGroqSuggestion() {
      switchToGroqBtn.style.display = "none";
    }

    function showGroqSuggestionIfAvailable() {
      const hasGroq = Array.from(providerAnalysisEl.options).some(x => x.value === "Groq");
      switchToGroqBtn.style.display = hasGroq ? "block" : "none";
    }

    function toApiProvider(value) {
      return value;
    }

    function saveLastProvider(provider) {
      if (!provider) return;
      localStorage.setItem(lastProviderStorageKey, provider);
    }

    function loadLastProvider() {
      return localStorage.getItem(lastProviderStorageKey) || "";
    }

    function updateModeloOptionsForProvider() {
      const provider = toApiProvider(providerAnalysisEl.value);
      const options = modelosPorProvedor[provider] || [{ value: "", label: "Padrão do provedor" }];
      const current = modeloIaEl.value;

      modeloIaEl.innerHTML = "";
      options.forEach(opt => {
        const option = document.createElement("option");
        option.value = opt.value;
        option.textContent = opt.label;
        modeloIaEl.appendChild(option);
      });

      if (options.some(x => x.value === current)) {
        modeloIaEl.value = current;
      }
    }

    function getSummaryStatusClass() {
      if (analysisStatus.className.includes("ok")) return "ok";
      if (analysisStatus.className.includes("bad")) return "bad";
      return "info";
    }

    function updateAnalysisConfigSummary() {
      const provider = toApiProvider(providerAnalysisEl.value) || "não definida";
      const status = (analysisStatus.text || "Pronto").trim();
      const statusClass = getSummaryStatusClass();
      analysisConfigSummaryEl.innerHTML = `Configuração da análise (IA atual: ${provider} | status: <span class="summary-status ${statusClass}">${status}</span>)`;
    }

    function setAnalysisState(text, chipClass) {
      analysisStatus = { text, className: chipClass };
      updateAnalysisConfigSummary();
    }

    function updateMaxTokensPlaceholderForProvider() {
      const provider = toApiProvider(providerAnalysisEl.value);
      const sugerido = maxTokensSugeridoPorProvedor[provider] ?? 1024;
      maxTokensIaEl.placeholder = `Padrão sugerido: ${sugerido}`;
    }

    function updateTemperaturaDefaultLabelForProvider() {
      const provider = toApiProvider(providerAnalysisEl.value);
      const sugerido = temperaturaPadraoPorProvedor[provider] ?? 0.2;
      if (temperaturaIaEl.options.length > 0) {
        temperaturaIaEl.options[0].textContent = `Padrão do provedor (${sugerido.toFixed(1)} - Conservador)`;
      }
    }

    function readDefaultsStore() {
      try {
        const raw = localStorage.getItem(advancedDefaultsStorageKey);
        if (!raw) return {};
        const parsed = JSON.parse(raw);
        return typeof parsed === "object" && parsed ? parsed : {};
      } catch {
        return {};
      }
    }

    function writeDefaultsStore(store) {
      localStorage.setItem(advancedDefaultsStorageKey, JSON.stringify(store));
    }

    function setAdvancedConfigFields(config) {
      modeloIaEl.value = config?.modelo || "";
      temperaturaIaEl.value = config?.temperatura ?? "";
      maxTokensIaEl.value = config?.maxTokens ?? "";
    }

    function applyDefaultsForCurrentProvider() {
      const provider = toApiProvider(providerAnalysisEl.value);
      const store = readDefaultsStore();
      updateModeloOptionsForProvider();
      updateTemperaturaDefaultLabelForProvider();
      updateMaxTokensPlaceholderForProvider();
      updateAnalysisConfigSummary();
      setAdvancedConfigFields(store[provider] || null);
    }

    function readAdvancedConfig() {
      const modelo = (modeloIaEl.value || "").trim();
      const temperaturaRaw = (temperaturaIaEl.value || "").trim();
      const maxTokensRaw = (maxTokensIaEl.value || "").trim();

      const hasTemperatura = temperaturaRaw.length > 0;
      const hasMaxTokens = maxTokensRaw.length > 0;

      const temperatura = hasTemperatura ? Number(temperaturaRaw) : null;
      const maxTokens = hasMaxTokens ? Number(maxTokensRaw) : null;

      if (hasTemperatura && (!Number.isFinite(temperatura) || temperatura < 0 || temperatura > 2)) {
        throw new Error("Temperatura inválida. Use um valor entre 0 e 2.");
      }

      if (hasMaxTokens && (!Number.isFinite(maxTokens) || maxTokens < 64 || maxTokens > 8192)) {
        throw new Error("Máx. tokens inválido. Use um valor entre 64 e 8192.");
      }

      if (!modelo && !hasTemperatura && !hasMaxTokens) {
        return null;
      }

      return {
        modelo: modelo || null,
        temperatura: hasTemperatura ? temperatura : null,
        maxTokens: hasMaxTokens ? Math.trunc(maxTokens) : null
      };
    }

    function saveAdvancedDefaults() {
      const provider = toApiProvider(providerAnalysisEl.value);
      const config = readAdvancedConfig();
      const store = readDefaultsStore();
      store[provider] = config;
      writeDefaultsStore(store);
      showToast(`Configurações padrão salvas para ${provider}.`);
    }

    function clearAdvancedDefaults() {
      const provider = toApiProvider(providerAnalysisEl.value);
      const store = readDefaultsStore();
      delete store[provider];
      writeDefaultsStore(store);
      setAdvancedConfigFields(null);
      showToast(`Padrões restaurados para ${provider}.`);
    }

    function renderList(el, list) {
      el.innerHTML = "";
      if (!Array.isArray(list) || list.length === 0) {
        const li = document.createElement("li");
        li.className = "muted";
        li.textContent = "Não identificado";
        el.appendChild(li);
        return;
      }

      list.forEach(item => {
        const li = document.createElement("li");
        li.textContent = item;
        el.appendChild(li);
      });
    }

    function renderActions(el, list) {
      el.innerHTML = "";
      if (!Array.isArray(list) || list.length === 0) {
        const li = document.createElement("li");
        li.className = "muted";
        li.textContent = "Não identificado";
        el.appendChild(li);
        return;
      }

      list.forEach(item => {
        const li = document.createElement("li");
        li.className = "action-item";
        const descricao = item?.descricao || "Sem descrição";
        const responsavel = item?.responsavel || "Não identificado";
        const prazo = item?.prazo || "";

        const meta = prazo ? `Responsável: ${responsavel} | Prazo: ${prazo}` : `Responsável: ${responsavel}`;
        li.innerHTML = `<div>${descricao}</div><div class="action-meta">${meta}</div>`;
        el.appendChild(li);
      });
    }

    function resetAnalysisOutput() {
      resumoEl.textContent = "Aguardando análise...";
      resumoEl.className = "value muted";
      renderList(topicosEl, []);
      renderActions(acoesEl, []);
      renderList(responsaveisEl, []);
      renderList(decisoesEl, []);
      renderList(pendenciasEl, []);
      dataReuniaoEl.textContent = "-";
      dataReuniaoEl.className = "value muted";
      tipoReuniaoEl.textContent = "-";
      tipoReuniaoEl.className = "value muted";
      confiancaEl.textContent = "-";
      confiancaEl.className = "value muted";
    }

    function addPdfText(doc, text, x, y, maxWidth, lineHeight) {
      const lines = doc.splitTextToSize(text || "Não informado", maxWidth);
      lines.forEach(line => {
        if (y > 276) {
          doc.addPage();
          y = 22;
        }

        doc.text(line, x, y);
        y += lineHeight;
      });

      return y;
    }

    function addPdfSection(doc, title, content, y) {
      if (y > 258) {
        doc.addPage();
        y = 22;
      }

      doc.setFont("helvetica", "bold");
      doc.setFontSize(11);
      doc.setTextColor(15, 23, 42);
      doc.text(title, 18, y);
      y += 7;

      doc.setFont("helvetica", "normal");
      doc.setFontSize(10);
      doc.setTextColor(51, 65, 85);
      return addPdfText(doc, content, 18, y, 174, 5) + 4;
    }

    function formatPdfList(items) {
      if (!Array.isArray(items) || items.length === 0) {
        return "Não identificado";
      }

      return items.map(item => `• ${item}`).join("\n");
    }

    function formatPdfActions(actions) {
      if (!Array.isArray(actions) || actions.length === 0) {
        return "Não identificado";
      }

      return actions.map((item, index) => {
        const descricao = item?.descricao || "Sem descrição";
        const responsavel = item?.responsavel || "Não identificado";
        const prazo = item?.prazo ? ` | Prazo: ${item.prazo}` : "";
        return `${index + 1}. ${descricao}\n   Responsável: ${responsavel}${prazo}`;
      }).join("\n");
    }

    function gerarPdfAnalise(analysis) {
      if (!analysis) {
        showToast("Registro inválido para geração do PDF.", true);
        return;
      }

      const jsPdf = window.jspdf?.jsPDF;
      if (!jsPdf) {
        showToast("Não foi possível carregar o gerador de PDF. Verifique a conexão com a internet e tente novamente.", true);
        return;
      }

      const doc = new jsPdf({ unit: "mm", format: "a4" });
      const generatedAt = new Date().toLocaleString("pt-BR");

      doc.setFillColor(239, 246, 255);
      doc.rect(0, 0, 210, 32, "F");
      doc.setFont("helvetica", "bold");
      doc.setFontSize(18);
      doc.setTextColor(15, 23, 42);
      doc.text("EasyMeet", 18, 17);
      doc.setFontSize(11);
      doc.setFont("helvetica", "normal");
      doc.setTextColor(71, 85, 105);
      doc.text("Relatório executivo de análise inteligente de reunião", 18, 25);

      let y = 44;
      doc.setFont("helvetica", "normal");
      doc.setFontSize(9);
      doc.setTextColor(100, 116, 139);
      doc.text(`Gerado em: ${generatedAt}`, 18, y);
      y += 6;
      doc.text(`IA utilizada: ${analysis.modoExecucao || "Não informada"}`, 18, y);
      y += 6;
      doc.text(`Data da reunião: ${formatDataReuniao(analysis.dataReuniao)}`, 18, y);
      y += 6;
      doc.text(`Tipo: ${analysis.tipoReuniao || "Não informado"} | Confiança: ${formatConfianca(analysis.nivelConfianca)}`, 18, y);
      y += 10;

      y = addPdfSection(doc, "Resumo executivo", analysis.resumo, y);
      y = addPdfSection(doc, "Tópicos principais", formatPdfList(analysis.topicosPrincipais), y);
      y = addPdfSection(doc, "Ações", formatPdfActions(analysis.acoes), y);
      y = addPdfSection(doc, "Responsáveis", formatPdfList(analysis.responsaveis), y);
      y = addPdfSection(doc, "Decisões", formatPdfList(analysis.decisoes), y);
      addPdfSection(doc, "Pendências", formatPdfList(analysis.pendencias), y);

      const dateSuffix = (analysis.dataReuniao || new Date().toISOString().slice(0, 10)).replaceAll("-", "");
      doc.save(`easymeet-analise-${dateSuffix}.pdf`);
    }

    function updateProviderStatusChip() {
      const provider = providerConfigEl.value;
      const configured = providerStatus[provider] === true;
      providerStatusEl.textContent = configured ? "Configurado" : "Não configurado";
      providerStatusEl.className = configured ? "chip ok" : "chip bad";
    }

    function renderSidebarProviders() {
      sidebarProvidersEl.innerHTML = "";
      if (!providersCatalog.length) {
        const empty = document.createElement("span");
        empty.className = "provider-pill";
        empty.textContent = "Nenhum provedor";
        sidebarProvidersEl.appendChild(empty);
        return;
      }

      providersCatalog.forEach(provider => {
        const providerKey = provider.provedorIA;
        const configured = providerStatus[providerKey] === true;
        const pill = document.createElement("span");
        pill.className = `provider-pill ${providerKey?.toString().toLowerCase() || ""} ${configured ? "configured" : "not-configured"}`;
        pill.textContent = provider.nome || providerKey;
        pill.title = configured ? "Configurado" : "Não configurado";
        sidebarProvidersEl.appendChild(pill);
      });
    }

    async function fetchProviders() {
      const resp = await fetch("/api/ia/provedores");
      if (!resp.ok) throw new Error("Falha ao carregar provedores.");
      const providers = await resp.json();
      providersCatalog = Array.isArray(providers) ? providers : [];
      providerConfigEl.innerHTML = "";
      providerAnalysisEl.innerHTML = "";
      providersCatalog.forEach(p => {
        const optionA = document.createElement("option");
        optionA.value = p.provedorIA;
        optionA.textContent = p.nome;
        providerConfigEl.appendChild(optionA);

        const optionB = document.createElement("option");
        optionB.value = p.provedorIA;
        optionB.textContent = p.nome;
        providerAnalysisEl.appendChild(optionB);
      });

      const lastProvider = loadLastProvider();
      renderSidebarProviders();

      if (lastProvider && providersCatalog.some(p => p.provedorIA === lastProvider)) {
        providerConfigEl.value = lastProvider;
        providerAnalysisEl.value = lastProvider;
      }
    }

    async function fetchStatus() {
      const resp = await fetch("/api/ia/credenciais/status");
      if (!resp.ok) throw new Error("Falha ao carregar status de credenciais.");
      const status = await resp.json();
      providerStatus = {};
      status.forEach(item => { providerStatus[item.provedorIA] = item.configurado; });
      updateProviderStatusChip();
      renderSidebarProviders();
    }

    async function saveOrUpdateKey() {
      const provedorIA = toApiProvider(providerConfigEl.value);
      const apiKey = apiKeyEl.value.trim();
      if (!apiKey) {
        showToast("Informe a API Key antes de salvar.", true);
        return;
      }

      const resp = await fetch("/api/ia/credenciais", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ provedorIA, apiKey })
      });

      if (!resp.ok) {
        showToast("Não foi possível salvar a chave.", true);
        return;
      }

      await fetchStatus();
      apiKeyEl.value = "";
      showToast("Chave salva com sucesso.");
    }

    async function testKey() {
      const provedorIA = toApiProvider(providerConfigEl.value);
      const apiKey = apiKeyEl.value.trim();
      if (!apiKey) {
        showToast("Informe uma API Key para testar a conexão.", true);
        return;
      }

      testKeyBtn.disabled = true;
      testKeyBtn.textContent = "Testando...";
      try {
        const resp = await fetch("/api/ia/credenciais/testar", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ provedorIA, apiKey })
        });

        if (!resp.ok) {
          showToast("Falha ao testar conexão.", true);
          return;
        }

        const data = await resp.json();
        if (data.sucesso) {
          showToast("Conexão válida.");
        } else {
          const detalhe = (data.mensagem || "").toString().trim();
          const amigavel = mapFriendlyErrorMessage(detalhe || "Conexão recusada.");
          const mensagemFinal = detalhe && amigavel !== detalhe
            ? `${amigavel} Detalhe técnico: ${detalhe}`
            : amigavel;
          showToast(mensagemFinal, true);
        }
      } finally {
        testKeyBtn.disabled = false;
        testKeyBtn.textContent = "Testar conexão";
      }
    }

    async function deleteKey() {
      const provedorIA = toApiProvider(providerConfigEl.value);
      const resp = await fetch(`/api/ia/credenciais/${provedorIA}`, { method: "DELETE" });
      if (!resp.ok) {
        showToast("Falha ao remover chave.", true);
        return;
      }
      await fetchStatus();
      showToast("Chave removida com sucesso.");
    }

    async function analisarReuniao() {
      hideGroqSuggestion();
      const transcricao = transcricaoEl.value.trim();
      const provedorIA = toApiProvider(providerAnalysisEl.value);
      if (!transcricao) {
        showToast("Informe a transcrição para análise.", true);
        return;
      }

      analisarBtn.disabled = true;
      analisarBtn.classList.add("is-loading");
      analisarBtn.textContent = "Analisando...";
      setAnalysisState("Processando", "chip info");
      resumoEl.textContent = "Processando...";
      resumoEl.className = "value";

      try {
        const configuracaoIA = readAdvancedConfig();
        const payload = { transcricao, provedorIA };
        if (configuracaoIA) {
          payload.configuracaoIA = configuracaoIA;
        }

        const resp = await fetch("/api/reunioes/analisar", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(payload)
        });

        if (!resp.ok) {
          let error = "Falha na análise.";
          try {
            const payload = await resp.json();
            error = payload.mensagem || payload.detail || error;
          } catch {}
          throw new Error(error);
        }

        const result = await resp.json();
        resumoEl.textContent = result.resumo || "Não informado";
        resumoEl.className = "value";
        renderList(topicosEl, result.topicosPrincipais);
        renderActions(acoesEl, result.acoes);
        renderList(responsaveisEl, result.responsaveis);
        renderList(decisoesEl, result.decisoes);
        renderList(pendenciasEl, result.pendencias);
        dataReuniaoEl.textContent = formatDataReuniao(result.dataReuniao);
        dataReuniaoEl.className = "value";
        tipoReuniaoEl.textContent = result.tipoReuniao || "Desconhecida";
        tipoReuniaoEl.className = "value";
        confiancaEl.textContent = formatConfianca(result.nivelConfianca);
        confiancaEl.className = "value";
        setAnalysisState("Concluído", "chip ok");
        historicoCarregado = false;
        if (!historicoViewEl.hidden) {
          await fetchHistorico(false);
        }
      } catch (err) {
        resetAnalysisOutput();
        setAnalysisState("Erro", "chip bad");
        const detalhe = (err?.message || "").toString().trim();
        const amigavel = mapFriendlyErrorMessage(detalhe || "Erro inesperado na análise.");
        const mensagemFinal = detalhe && amigavel !== detalhe
          ? `${amigavel} Detalhe técnico: ${detalhe}`
          : amigavel;
        if (shouldSuggestGroqSwitch(detalhe)) {
          showGroqSuggestionIfAvailable();
        }
        showToast(mensagemFinal, true);
      } finally {
        analisarBtn.disabled = false;
        analisarBtn.classList.remove("is-loading");
        analisarBtn.textContent = "Analisar reunião";
      }
    }

    tabAnaliseBtn.addEventListener("click", () => setActiveTab("analise"));
    tabHistoricoBtn.addEventListener("click", () => setActiveTab("historico"));
    refreshHistoricoBtn.addEventListener("click", () => fetchHistorico());
    historicoFiltroTextoEl.addEventListener("input", () => {
      historicoPaginaAtual = 1;
      aplicarFiltrosHistorico();
    });
    historicoFiltroDataEl.addEventListener("change", () => {
      historicoPaginaAtual = 1;
      aplicarFiltrosHistorico();
    });
    historicoFiltroTipoEl.addEventListener("change", () => {
      historicoPaginaAtual = 1;
      aplicarFiltrosHistorico();
    });
    limparFiltrosHistoricoBtn.addEventListener("click", limparFiltrosHistorico);
    historicoPaginaAnteriorBtn.addEventListener("click", () => {
      historicoPaginaAtual = Math.max(1, historicoPaginaAtual - 1);
      aplicarFiltrosHistorico();
    });
    historicoProximaPaginaBtn.addEventListener("click", () => {
      historicoPaginaAtual += 1;
      aplicarFiltrosHistorico();
    });
    providerConfigEl.addEventListener("change", updateProviderStatusChip);
    providerConfigEl.addEventListener("change", () => saveLastProvider(toApiProvider(providerConfigEl.value)));
    toggleConfigBtn.addEventListener("click", toggleConfigDrawer);
    configBackdropEl.addEventListener("click", closeConfigDrawer);
    saveKeyBtn.addEventListener("click", saveOrUpdateKey);
    testKeyBtn.addEventListener("click", testKey);
    deleteKeyBtn.addEventListener("click", deleteKey);
    analisarBtn.addEventListener("click", analisarReuniao);
    saveDefaultsIaBtn.addEventListener("click", () => {
      try {
        saveAdvancedDefaults();
      } catch (err) {
        showToast(err.message || "Falha ao salvar padrões avançados.", true);
      }
    });
    clearDefaultsIaBtn.addEventListener("click", clearAdvancedDefaults);
    providerAnalysisEl.addEventListener("change", () => {
      saveLastProvider(toApiProvider(providerAnalysisEl.value));
      applyDefaultsForCurrentProvider();
      hideGroqSuggestion();
    });
    switchToGroqBtn.addEventListener("click", () => {
      const hasGroq = Array.from(providerAnalysisEl.options).some(x => x.value === "Groq");
      if (!hasGroq) {
        showToast("Provedor Groq não está disponível nesta instância.", true);
        return;
      }

      providerAnalysisEl.value = "Groq";
      providerConfigEl.value = "Groq";
      providerConfigEl.dispatchEvent(new Event("change"));
      providerAnalysisEl.dispatchEvent(new Event("change"));
      showToast("Provedor alterado para Groq. Você já pode analisar novamente.");
    });
    document.addEventListener("keydown", event => {
      if (event.key === "Escape" && configDrawerEl.classList.contains("open")) {
        closeConfigDrawer();
      }
    });

    async function bootstrap() {
      resetAnalysisOutput();
      await fetchProviders();
      await fetchStatus();
      applyDefaultsForCurrentProvider();
      setAnalysisState("Pronto", "chip info");
    }

    bootstrap().catch(err => showToast(err.message || "Falha na inicialização.", true));
