using EasyMeet.Api.Models;

namespace EasyMeet.Api.Services;

public interface IGenerativeAIClient
{
    ProvedorIA Provedor { get; }
    Task<string> GerarConteudoAsync(
        string prompt,
        string apiKey,
        ConfiguracaoAnaliseIA? configuracaoIA,
        CancellationToken cancellationToken);
    Task<bool> TestarConexaoAsync(string apiKey, CancellationToken cancellationToken);
}
