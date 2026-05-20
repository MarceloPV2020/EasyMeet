const btn = document.getElementById('enviar');
const resultSection = document.getElementById('resultado');
const toast = document.getElementById('toast');
const input = document.getElementById('texto');

const fields = {
  tipoReuniao: document.getElementById('tipoReuniao'),
  nivelConfianca: document.getElementById('nivelConfianca'),
  topicos: document.getElementById('topicos'),
  acoes: document.getElementById('acoes'),
  responsaveis: document.getElementById('responsaveis'),
  saida: document.getElementById('saida')
};

let toastTimeout;

function showError(msg) {
  console.error('Erro detectado:', msg);
  toast.textContent = msg;
  toast.style.display = 'block';

  if (toastTimeout) clearTimeout(toastTimeout);
  toastTimeout = setTimeout(() => {
    toast.style.display = 'none';
  }, 8000);
}

function clearResults() {
  Object.values(fields).forEach(f => f.value = '');
  resultSection.style.display = 'none';
}

btn.addEventListener('click', async () => {
  const texto = input.value.trim();
  if (!texto) {
    showError('Por favor, insira o texto da reunião.');
    return;
  }

  btn.disabled = true;
  btn.textContent = 'Processando...';
  toast.style.display = 'none';
  clearResults();

  try {
    const resp = await fetch('/api/reunioes/resumir', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ texto })
    });

    if (resp.ok) {
      const data = await resp.json();

      fields.tipoReuniao.value = data.tipoReuniao || 'N/A';
      fields.nivelConfianca.value = (data.nivelConfianca * 100).toFixed(1) + '%';
      fields.topicos.value = (data.topicosPrincipais || []).join('\n');
      fields.acoes.value = (data.acoes || []).join('\n');
      fields.responsaveis.value = (data.responsaveis || []).join(', ');
      fields.saida.value = data.resumo || '';

      resultSection.style.display = 'block';
    } else {
      let errorMsg = 'Erro desconhecido ao processar resumo.';
      try {
        const errorData = await resp.json();
        errorMsg = errorData.mensagem || errorData.detail || errorMsg;
      } catch (jsonErr) {
        try {
          const textErr = await resp.text();
          if (textErr) errorMsg = textErr;
        } catch (textErr) { }
      }
      showError(errorMsg);
    }
  } catch (e) {
    showError('Falha na comunicação com o servidor: ' + e.message);
  } finally {
    btn.disabled = false;
    btn.textContent = 'Resumir Reunião';
  }
});
