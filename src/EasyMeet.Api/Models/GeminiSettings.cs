namespace EasyMeet.Api.Models;

/// <summary>
/// Configurações de integração com Gemini.
/// </summary>
public sealed class GeminiSettings
{
    /// <summary>
    /// Nome da seção no appsettings.
    /// </summary>
    public const string SectionName = "Gemini";

    /// <summary>
    /// Chave de API para integração real.
    /// </summary>
    public string ApiKey { get; init; } = string.Empty;

    /// <summary>
    /// Modelo Gemini desejado.
    /// </summary>
    public string Model { get; init; } = "gemini-1.5-flash";

    /// <summary>
    /// Modo de execução atual, ex: Simulado ou Producao.
    /// </summary>
    public string ModoExecucao { get; init; } = "Simulado";
}
