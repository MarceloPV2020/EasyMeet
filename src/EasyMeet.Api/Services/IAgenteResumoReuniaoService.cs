using EasyMeet.Api.Models;

namespace EasyMeet.Api.Services;

/// <summary>
/// Contrato para servico de analise e resumo de reunioes usando IA.
/// </summary>
public interface IAgenteResumoReuniaoService
{
    /// <summary>
    /// Analisa o texto da reuniao e retorna o resultado estruturado.
    /// </summary>
    Task<ResumoReuniaoResponse> ResumirAsync(string texto, CancellationToken cancellationToken = default);
}
