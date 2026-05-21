namespace EasyMeet.Api.Models;

/// <summary>
/// Resposta estruturada da analise de reuniao gerada com apoio de IA.
/// </summary>
public sealed class ResumoReuniaoResponse
{
    public string Resumo { get; init; } = string.Empty;
    public IReadOnlyList<string> TopicosPrincipais { get; init; } = [];
    public IReadOnlyList<AcaoReuniaoItem> Acoes { get; init; } = [];
    public IReadOnlyList<string> Responsaveis { get; init; } = [];
    public IReadOnlyList<string> Decisoes { get; init; } = [];
    public IReadOnlyList<string> Pendencias { get; init; } = [];
    public string DataReuniao { get; init; } = string.Empty;
    public string TipoReuniao { get; init; } = string.Empty;
    public double NivelConfianca { get; init; }
    public bool GeradoPorIA { get; init; }
    public string ModoExecucao { get; init; } = string.Empty;
    public string ModeloIA { get; init; } = string.Empty;
}
