namespace EasyMeet.Api.Models;

public sealed class GeminiSettings
{
    public const string SectionName = "Gemini";

    public string Model { get; init; } = "gemini-2.5-flash";
}
