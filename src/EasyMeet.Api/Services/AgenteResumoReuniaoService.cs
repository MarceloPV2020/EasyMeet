using EasyMeet.Api.Models;
using EasyMeet.Api.Prompts;

namespace EasyMeet.Api.Services;

/// <summary>
/// Serviço de orquestração de resumo de reunião com IA.
/// </summary>
public sealed class AgenteResumoReuniaoService(
    PromptResumoReuniaoBuilder promptBuilder,
    GeminiClientService geminiClientService,
    ILogger<AgenteResumoReuniaoService> logger) : IAgenteResumoReuniaoService
{
    /// <inheritdoc />
    public async Task<ResumoReuniaoResponse> ResumirAsync(string texto, CancellationToken cancellationToken = default)
    {
        var prompt = await promptBuilder.BuildAsync(texto, cancellationToken);
        var iaResult = await geminiClientService.TryGerarResumoAsync(prompt, cancellationToken);

        if (iaResult is not null)
        {
            return new ResumoReuniaoResponse
            {
                Resumo = iaResult.Resumo,
                TopicosPrincipais = iaResult.TopicosPrincipais,
                Acoes = iaResult.Acoes,
                Responsaveis = iaResult.Responsaveis,
                TipoReuniao = iaResult.TipoReuniao,
                NivelConfianca = iaResult.NivelConfianca,
                GeradoPorIA = true,
                ModoExecucao = "gemini"
            };
        }

        logger.LogInformation("Executando fallback local para resumo de reunião.");
        return BuildFallbackResponse(texto);
    }

    private static ResumoReuniaoResponse BuildFallbackResponse(string texto)
    {
        var resumo = texto.Length > 280 ? texto[..280] + "..." : texto;

        return new ResumoReuniaoResponse
        {
            Resumo = $"Resumo local (fallback): {resumo}",
            TopicosPrincipais = ["Resumo indisponível na IA", "Análise local simplificada"],
            Acoes = ["Revisar manualmente os pontos de ação"],
            Responsaveis = ["Não identificado"],
            TipoReuniao = "Unknown",
            NivelConfianca = 0.35,
            GeradoPorIA = false,
            ModoExecucao = "fallback_local"
        };
    }
}
