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

        return new ResumoReuniaoResponse
        {
            Resumo = parsed.Resumo,
            TopicosPrincipais = parsed.TopicosPrincipais,
            Acoes = parsed.Acoes,
            Responsaveis = parsed.Responsaveis,
            Decisoes = parsed.Decisoes,
            Pendencias = parsed.Pendencias,
            TipoReuniao = parsed.TipoReuniao,
            NivelConfianca = parsed.NivelConfianca,
            GeradoPorIA = true,
            ModoExecucao = provedorIA.ToString()
        };
    }
}
