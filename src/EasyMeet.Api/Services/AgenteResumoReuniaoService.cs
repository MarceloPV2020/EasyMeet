using EasyMeet.Api.Models;
using EasyMeet.Api.Prompts;

namespace EasyMeet.Api.Services;

/// <summary>
/// Serviço de orquestração de resumo de reunião com IA.
/// </summary>
public sealed class AgenteResumoReuniaoService(
    PromptResumoReuniaoBuilder promptBuilder,
    GeminiClientService geminiClientService) : IAgenteResumoReuniaoService
{
    /// <inheritdoc />
    public async Task<ResumoReuniaoResponse> ResumirAsync(string texto, CancellationToken cancellationToken = default)
    {
        var prompt = await promptBuilder.BuildAsync(texto, cancellationToken);
        var iaResult = await geminiClientService.TryGerarResumoAsync(prompt, cancellationToken);

        // Se chegar aqui sem exceção, iaResult não será nulo devido às mudanças no GeminiClientService
        return new ResumoReuniaoResponse
        {
            Resumo = iaResult!.Resumo,
            TopicosPrincipais = iaResult.TopicosPrincipais,
            Acoes = iaResult.Acoes,
            Responsaveis = iaResult.Responsaveis,
            TipoReuniao = iaResult.TipoReuniao,
            NivelConfianca = iaResult.NivelConfianca,
            GeradoPorIA = true,
            ModoExecucao = "gemini"
        };
    }
}
