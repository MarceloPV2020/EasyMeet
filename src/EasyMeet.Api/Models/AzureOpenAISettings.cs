namespace EasyMeet.Api.Models;

public sealed class AzureOpenAISettings
{
    public const string SectionName = "AzureOpenAI";
    public string Endpoint { get; init; } = string.Empty;
    public string Deployment { get; init; } = "gpt-4.1-mini";
    public string ApiVersion { get; init; } = "2024-10-21";
}

