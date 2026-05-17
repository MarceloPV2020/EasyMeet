namespace EasyMeet.Api.Models;

/// <summary>
/// Resposta estruturada da análise de reunião gerada com apoio de IA.
/// </summary>
public sealed class ResumoReuniaoResponse
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
    /// Ações identificadas na reunião.
    /// </summary>
    public IReadOnlyList<string> Acoes { get; init; } = [];

    /// <summary>
    /// Responsáveis inferidos para as ações.
    /// </summary>
    public IReadOnlyList<string> Responsaveis { get; init; } = [];

    /// <summary>
    /// Classificação do tipo de reunião.
    /// </summary>
    public string TipoReuniao { get; init; } = string.Empty;

    /// <summary>
    /// Nível de confiança da análise no intervalo de 0 a 1.
    /// </summary>
    public double NivelConfianca { get; init; }

    /// <summary>
    /// Indica se o resultado foi gerado por IA.
    /// </summary>
    public bool GeradoPorIA { get; init; }

    /// <summary>
    /// Informa o modo de execução do serviço de IA.
    /// </summary>
    public string ModoExecucao { get; init; } = string.Empty;
}
