namespace EasyMeet.Api.Models;

public sealed class OpenAISettings
{
    public const string SectionName = "OpenAI";
    public string Model { get; init; } = "gpt-4.1-mini";
}

