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
    public async Task<string> ResumirAsync(string texto, CancellationToken cancellationToken = default)
    {
        var prompt = await promptBuilder.BuildAsync(texto, cancellationToken);
        var iaResult = await geminiClientService.TryGerarResumoAsync(prompt, cancellationToken);

        // Se chegar aqui sem exceção, iaResult não será nulo devido às mudanças no GeminiClientService
        return iaResult!.Resumo;
    }
}
