namespace EasyMeet.Api.Models;

public sealed class GroqSettings
{
    public const string SectionName = "Groq";
    public string Model { get; init; } = "llama-3.1-8b-instant";
}
