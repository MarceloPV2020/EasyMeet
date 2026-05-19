namespace EasyMeet.Api.Services;

/// <summary>
/// Contrato para serviço de análise e resumo de reuniões usando IA.
/// </summary>
public interface IAgenteResumoReuniaoService
{
    /// <summary>
    /// Analisa o texto da reunião e retorna o resumo em formato de texto.
    /// </summary>
    Task<string> ResumirAsync(string texto, CancellationToken cancellationToken = default);
}
