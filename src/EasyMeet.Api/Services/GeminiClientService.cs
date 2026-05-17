using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EasyMeet.Api.Models;

namespace EasyMeet.Api.Services;

/// <summary>
/// Cliente responsável por comunicação com Google Gemini.
/// </summary>
public sealed class GeminiClientService(HttpClient httpClient, ILogger<GeminiClientService> logger)
{
    private const string ModelName = "gemini-2.5-flash";
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(25);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Executa análise com Gemini e retorna o JSON estruturado quando válido.
    /// </summary>
    public async Task<ResumoReuniaoIaOutput?> TryGerarResumoAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("GEMINI_API_KEY não configurada. IA real desativada.");
            return null;
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(RequestTimeout);

        try
        {
            using var request = BuildRequest(prompt, apiKey);
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeoutCts.Token);
            var responseContent = await response.Content.ReadAsStringAsync(timeoutCts.Token);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Gemini retornou status {StatusCode}. Body: {Body}", (int)response.StatusCode, responseContent);
                return null;
            }

            var candidateText = ExtractCandidateText(responseContent);
            if (string.IsNullOrWhiteSpace(candidateText))
            {
                logger.LogWarning("Gemini não retornou conteúdo textual utilizável.");
                return null;
            }

            var cleanedJson = TryExtractJsonObject(candidateText);
            if (string.IsNullOrWhiteSpace(cleanedJson))
            {
                logger.LogWarning("Não foi possível extrair JSON válido da resposta do Gemini.");
                return null;
            }

            var parsed = DeserializeAndValidate(cleanedJson);
            if (parsed is null)
            {
                logger.LogWarning("JSON do Gemini inválido ou fora do schema esperado.");
                return null;
            }

            logger.LogInformation("Resposta válida recebida do Gemini.");
            return parsed;
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Timeout ao chamar Gemini após {TimeoutSeconds}s.", RequestTimeout.TotalSeconds);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha inesperada ao chamar Gemini.");
            return null;
        }
    }

    private static HttpRequestMessage BuildRequest(string prompt, string apiKey)
    {
        var uri = $"v1beta/models/{ModelName}:generateContent";

        var body = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = prompt
                        }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.2,
                responseMimeType = "application/json"
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("x-goog-api-key", apiKey);

        return request;
    }

    private static string? ExtractCandidateText(string responseContent)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseContent);

            var root = doc.RootElement;
            if (!root.TryGetProperty("candidates", out var candidates) || candidates.ValueKind != JsonValueKind.Array || candidates.GetArrayLength() == 0)
            {
                return null;
            }

            var first = candidates[0];
            if (!first.TryGetProperty("content", out var content))
            {
                return null;
            }

            if (!content.TryGetProperty("parts", out var parts) || parts.ValueKind != JsonValueKind.Array || parts.GetArrayLength() == 0)
            {
                return null;
            }

            var text = parts[0].GetProperty("text").GetString();
            return text;
        }
        catch
        {
            return null;
        }
    }

    private static string? TryExtractJsonObject(string rawText)
    {
        var cleaned = rawText.Trim();
        if (cleaned.StartsWith("```", StringComparison.Ordinal))
        {
            cleaned = cleaned.Replace("```json", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("```", string.Empty, StringComparison.Ordinal)
                .Trim();
        }

        var start = cleaned.IndexOf('{');
        var end = cleaned.LastIndexOf('}');

        if (start < 0 || end < 0 || end <= start)
        {
            return null;
        }

        return cleaned[start..(end + 1)];
    }

    private static ResumoReuniaoIaOutput? DeserializeAndValidate(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!HasExpectedShape(root))
            {
                return null;
            }

            var result = JsonSerializer.Deserialize<ResumoReuniaoIaOutput>(json, JsonOptions);
            if (result is null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(result.Resumo))
            {
                return null;
            }

            var confidence = Math.Clamp(result.NivelConfianca, 0d, 1d);
            return new ResumoReuniaoIaOutput
            {
                Resumo = result.Resumo.Trim(),
                TopicosPrincipais = result.TopicosPrincipais.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                Acoes = result.Acoes.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                Responsaveis = result.Responsaveis.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                TipoReuniao = string.IsNullOrWhiteSpace(result.TipoReuniao) ? "Unknown" : result.TipoReuniao.Trim(),
                NivelConfianca = confidence
            };
        }
        catch
        {
            return null;
        }
    }

    private static bool HasExpectedShape(JsonElement root)
    {
        return root.ValueKind == JsonValueKind.Object
            && root.TryGetProperty("resumo", out var resumo) && resumo.ValueKind == JsonValueKind.String
            && root.TryGetProperty("topicosPrincipais", out var topicos) && topicos.ValueKind == JsonValueKind.Array
            && root.TryGetProperty("acoes", out var acoes) && acoes.ValueKind == JsonValueKind.Array
            && root.TryGetProperty("responsaveis", out var responsaveis) && responsaveis.ValueKind == JsonValueKind.Array
            && root.TryGetProperty("tipoReuniao", out var tipo) && tipo.ValueKind == JsonValueKind.String
            && root.TryGetProperty("nivelConfianca", out var confianca) && (confianca.ValueKind == JsonValueKind.Number || confianca.ValueKind == JsonValueKind.String);
    }
}
