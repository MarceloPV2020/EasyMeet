using EasyMeet.Api.Models;

namespace EasyMeet.Api.Services;

public sealed class ApiKeyManagerService(IApiKeyStore apiKeyStore, IAProviderFactory providerFactory)
{
    public async Task SalvarOuAtualizarAsync(ProvedorIA provedorIA, string apiKey, CancellationToken cancellationToken)
    {
        _ = providerFactory.GetClient(provedorIA);
        await apiKeyStore.SaveApiKeyAsync(provedorIA, apiKey.Trim(), cancellationToken);
    }

    public Task RemoverAsync(ProvedorIA provedorIA, CancellationToken cancellationToken)
    {
        return apiKeyStore.DeleteApiKeyAsync(provedorIA, cancellationToken);
    }

    public async Task<IReadOnlyList<ApiKeyStatusResponse>> ObterStatusAsync(CancellationToken cancellationToken)
    {
        var items = new List<ApiKeyStatusResponse>();
        foreach (var provedor in providerFactory.ListarProvedores())
        {
            items.Add(new ApiKeyStatusResponse
            {
                ProvedorIA = provedor,
                Configurado = await apiKeyStore.HasApiKeyAsync(provedor, cancellationToken)
            });
        }

        return items;
    }

    public async Task<bool> TestarConexaoAsync(ProvedorIA provedorIA, string apiKey, CancellationToken cancellationToken)
    {
        var provider = providerFactory.GetClient(provedorIA);
        return await provider.TestarConexaoAsync(apiKey.Trim(), cancellationToken);
    }

    public async Task<ResultadoTesteConexaoResponse> TestarConexaoDetalhadoAsync(ProvedorIA provedorIA, string apiKey, CancellationToken cancellationToken)
    {
        try
        {
            var provider = providerFactory.GetClient(provedorIA);
            _ = await provider.GerarConteudoAsync("Responda apenas OK", apiKey.Trim(), null, cancellationToken);
            return new ResultadoTesteConexaoResponse { Sucesso = true };
        }
        catch (Exception ex)
        {
            return new ResultadoTesteConexaoResponse
            {
                Sucesso = false,
                Mensagem = ex.Message
            };
        }
    }
}
