namespace EasyMeet.Api.Models;

public sealed class AnthropicSettings
{
    public const string SectionName = "Anthropic";
    public string Model { get; init; } = "claude-3-7-sonnet-20250219";
}

