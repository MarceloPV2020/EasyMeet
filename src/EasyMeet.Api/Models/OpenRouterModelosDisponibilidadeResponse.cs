namespace EasyMeet.Api.Models;

public sealed class OpenRouterModelosDisponibilidadeResponse
{
    public IReadOnlyList<string> ModelosDisponiveis { get; init; } = [];
    public IReadOnlyList<string> ModelosIndisponiveis { get; init; } = [];
}
