namespace EasyMeet.Api.Models;

public sealed class ConfiguracaoAnaliseIA
{
    public string? Modelo { get; init; }
    public double? Temperatura { get; init; }
    public int? MaxTokens { get; init; }
}

