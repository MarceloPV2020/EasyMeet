namespace EasyMeet.Api.Models;

/// <summary>
/// Registro persistido de uma analise de reuniao.
/// </summary>
public sealed class Reuniao
{
    public int Id { get; init; }
    public string Transcricao { get; init; } = string.Empty;
    public string Resumo { get; init; } = string.Empty;
    public decimal Confianca { get; init; }
}
