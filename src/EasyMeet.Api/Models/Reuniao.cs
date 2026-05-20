namespace EasyMeet.Api.Models;

/// <summary>
/// Registro persistido de uma analise de reuniao.
/// </summary>
public sealed class Reuniao
{
    public int Id { get; init; }
    public string Transcricao { get; init; } = string.Empty;
    public string Resumo { get; init; } = string.Empty;
    public IReadOnlyList<string> TopicosPrincipais { get; init; } = [];
    public IReadOnlyList<AcaoReuniaoItem> Acoes { get; init; } = [];
    public IReadOnlyList<string> Responsaveis { get; init; } = [];
    public IReadOnlyList<string> Decisoes { get; init; } = [];
    public IReadOnlyList<string> Pendencias { get; init; } = [];
    public string DataReuniao { get; init; } = string.Empty;
    public string TipoReuniao { get; init; } = string.Empty;
    public decimal Confianca { get; init; }
    public bool GeradoPorIA { get; init; }
    public string ModoExecucao { get; init; } = string.Empty;
}
