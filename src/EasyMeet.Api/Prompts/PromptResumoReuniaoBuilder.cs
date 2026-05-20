using System.Text.Json;

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
    public async Task<string> BuildAsync(string texto, CancellationToken cancellationToken = default)
    {
        var promptTemplate = await LoadPromptTemplateAsync(cancellationToken);
        return promptTemplate.Replace("{{TRANSCRICAO}}", texto, StringComparison.Ordinal);
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
            decisoes = Array.Empty<string>(),
            pendencias = Array.Empty<string>(),
            dataReuniao = string.Empty,
            tipoReuniao = "Desconhecida",
            nivelConfianca = 0.0
        });

        return $"Retorne apenas JSON válido neste formato: {schema}. Idioma: pt-BR. Transcrição: {{{{TRANSCRICAO}}}}";
    }
}
