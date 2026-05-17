using System.Text.Json;
using EasyMeet.Api.Models;

namespace EasyMeet.Api.Prompts;

/// <summary>
/// Builder de prompt para análise de transcrição de reunião.
/// </summary>
public sealed class PromptResumoReuniaoBuilder(ILogger<PromptResumoReuniaoBuilder> logger, IWebHostEnvironment environment)
{
    private const string PromptRelativePath = "Prompts/resumo-reuniao-agent-prompt.txt";

    /// <summary>
    /// Monta prompt estruturado para o modelo de IA.
    /// </summary>
    public async Task<string> BuildAsync(string texto, string idioma, CancellationToken cancellationToken = default)
    {
        var promptTemplate = await LoadPromptTemplateAsync(cancellationToken);
        return promptTemplate
            .Replace("{{TRANSCRICAO}}", texto, StringComparison.Ordinal)
            .Replace("{{IDIOMA}}", idioma, StringComparison.Ordinal);
    }

    private async Task<string> LoadPromptTemplateAsync(CancellationToken cancellationToken)
    {
        var promptPath = Path.Combine(environment.ContentRootPath, PromptRelativePath);

        if (!File.Exists(promptPath))
        {
            logger.LogWarning("Prompt file not found at {PromptPath}. Using embedded fallback template.", promptPath);
            return GetFallbackTemplate();
        }

        try
        {
            return await File.ReadAllTextAsync(promptPath, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to read prompt file at {PromptPath}. Using embedded fallback template.", promptPath);
            return GetFallbackTemplate();
        }
    }

    private static string GetFallbackTemplate()
    {
        var schema = JsonSerializer.Serialize(new
        {
            resumo = string.Empty,
            topicosPrincipais = Array.Empty<string>(),
            acoes = Array.Empty<string>(),
            responsaveis = Array.Empty<string>(),
            tipoReuniao = "Unknown",
            nivelConfianca = 0.0
        });

        return $"Retorne apenas JSON válido neste formato: {schema}. Idioma: {{IDIOMA}}. Transcrição: {{TRANSCRICAO}}";
    }
}
