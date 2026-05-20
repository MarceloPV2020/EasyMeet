namespace EasyMeet.Api.Models;

/// <summary>
/// Estrutura obrigatória de saída da IA para resumo de reunião.
/// </summary>
public sealed class ResumoReuniaoIaOutput
{
    /// <summary>
    /// Resumo consolidado da reunião.
    /// </summary>
    public string Resumo { get; init; } = string.Empty;

    /// <summary>
    /// Tópicos principais identificados.
    /// </summary>
    public IReadOnlyList<string> TopicosPrincipais { get; init; } = [];

    /// <summary>
    /// Ações identificadas.
    /// </summary>
    public IReadOnlyList<AcaoReuniaoItem> Acoes { get; init; } = [];

    /// <summary>
    /// Responsáveis identificados.
    /// </summary>
    public IReadOnlyList<string> Responsaveis { get; init; } = [];

    /// <summary>
    /// Decisões tomadas na reunião.
    /// </summary>
    public IReadOnlyList<string> Decisoes { get; init; } = [];

    /// <summary>
    /// Pendências identificadas.
    /// </summary>
    public IReadOnlyList<string> Pendencias { get; init; } = [];

    /// <summary>
    /// Classificação do tipo de reunião.
    /// </summary>
    public string TipoReuniao { get; init; } = "Unknown";

    /// <summary>
    /// Nível de confiança de 0 a 1.
    /// </summary>
    public double NivelConfianca { get; init; }
}
