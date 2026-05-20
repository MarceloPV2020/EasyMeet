namespace EasyMeet.Api.Models;

public sealed class ApiKeyStatusResponse
{
    public ProvedorIA ProvedorIA { get; init; }
    public bool Configurado { get; init; }
}
