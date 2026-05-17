namespace EasyMeet.Api.Services;

/// <summary>
/// Contrato para serviço de análise e resumo de reuniões usando IA.
/// </summary>
public interface IAgenteResumoReuniaoService
{
    /// <summary>
    /// Analisa o texto da reunião e retorna o resultado estruturado.
    /// </summary>
    Task<Models.ResumoReuniaoResponse> ResumirAsync(string texto, string idioma, CancellationToken cancellationToken = default);
}
