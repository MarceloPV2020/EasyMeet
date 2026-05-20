using EasyMeet.Api.Models;

namespace EasyMeet.Api.Services;

public interface IAgenteResumoReuniaoService
{
    Task<ResumoReuniaoResponse> ResumirAsync(
        string transcricao,
        ProvedorIA provedorIA,
        ConfiguracaoAnaliseIA? configuracaoIA = null,
        CancellationToken cancellationToken = default);
}
