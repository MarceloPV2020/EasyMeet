namespace EasyMeet.Api.Models;

public sealed class OpenRouterSettings
{
    public const string SectionName = "OpenRouter";
    public string Model { get; init; } = "google/gemini-2.5-flash";
}
