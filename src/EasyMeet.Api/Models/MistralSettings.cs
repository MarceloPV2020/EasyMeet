namespace EasyMeet.Api.Models;

public sealed class MistralSettings
{
    public const string SectionName = "Mistral";
    public string Model { get; init; } = "mistral-small-latest";
}

