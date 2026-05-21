using EasyMeet.Api.Models;
using EasyMeet.Api.Prompts;

namespace EasyMeet.Api.Services;

public sealed class AgenteResumoReuniaoService(
    PromptResumoReuniaoBuilder promptBuilder,
    IApiKeyStore apiKeyStore,
    IAProviderFactory providerFactory,
    MeetingAnalysisParser parser) : IAgenteResumoReuniaoService
{
    public async Task<ResumoReuniaoResponse> ResumirAsync(
        string transcricao,
        ProvedorIA provedorIA,
        ConfiguracaoAnaliseIA? configuracaoIA = null,
        CancellationToken cancellationToken = default)
    {
        var apiKey = await apiKeyStore.GetApiKeyAsync(provedorIA, cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Chave de API nao configurada para o provedor selecionado.");
        }

        var provider = providerFactory.GetClient(provedorIA);
        var prompt = await promptBuilder.BuildAsync(transcricao, cancellationToken);
        var rawContent = await provider.GerarConteudoAsync(prompt, apiKey, configuracaoIA, cancellationToken);
        var parsed = parser.ParseOrThrow(rawContent);
        var modeloEfetivo = ResolveModeloEfetivo(provedorIA, configuracaoIA);

        return new ResumoReuniaoResponse
        {
            Resumo = parsed.Resumo,
            TopicosPrincipais = parsed.TopicosPrincipais,
            Acoes = parsed.Acoes,
            Responsaveis = parsed.Responsaveis,
            Decisoes = parsed.Decisoes,
            Pendencias = parsed.Pendencias,
            DataReuniao = parsed.DataReuniao,
            TipoReuniao = parsed.TipoReuniao,
            NivelConfianca = parsed.NivelConfianca,
            GeradoPorIA = true,
            ModoExecucao = provedorIA.ToString(),
            ModeloIA = modeloEfetivo
        };
    }

    private static string ResolveModeloEfetivo(ProvedorIA provedorIA, ConfiguracaoAnaliseIA? configuracaoIA)
    {
        if (!string.IsNullOrWhiteSpace(configuracaoIA?.Modelo))
        {
            return configuracaoIA.Modelo.Trim();
        }

        return provedorIA switch
        {
            ProvedorIA.Gemini => "gemini-2.5-flash",
            ProvedorIA.Groq => "llama-3.1-8b-instant",
            ProvedorIA.OpenAI => "gpt-4.1-mini",
            ProvedorIA.Anthropic => "claude-3-7-sonnet-20250219",
            ProvedorIA.Mistral => "mistral-small-latest",
            ProvedorIA.Cohere => "command-a-03-2025",
            ProvedorIA.OpenRouter => "google/gemini-2.5-flash",
            _ => "modelo-nao-identificado"
        };
    }
}
