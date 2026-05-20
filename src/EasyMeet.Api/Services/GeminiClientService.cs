using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EasyMeet.Api.Models;
using Microsoft.Extensions.Options;

namespace EasyMeet.Api.Services;

/// <summary>
/// Cliente responsável por comunicação com Google Gemini.
/// </summary>
public sealed class GeminiClientService(
    HttpClient httpClient, 
    IOptions<GeminiSettings> geminiOptions,
    ILogger<GeminiClientService> logger)
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
        var apiKey = geminiOptions.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            const string msg = "GEMINI_API_KEY não configurada no appsettings.json. IA real desativada.";
            logger.LogWarning(msg);
            throw new InvalidOperationException(msg);
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
                var msg = $"Gemini retornou status {(int)response.StatusCode}. Body: {responseContent}";
                logger.LogWarning(msg);
                throw new InvalidOperationException(msg);
            }

            var candidateText = ExtractCandidateText(responseContent);
            if (string.IsNullOrWhiteSpace(candidateText))
            {
                const string msg = "Gemini não retornou conteúdo textual utilizável.";
                logger.LogWarning(msg);
                throw new InvalidOperationException(msg);
            }

            var cleanedJson = TryExtractJsonObject(candidateText);
            if (string.IsNullOrWhiteSpace(cleanedJson))
            {
                const string msg = "Não foi possível extrair JSON válido da resposta do Gemini.";
                logger.LogWarning(msg);
                throw new InvalidOperationException(msg);
            }

            var parsed = DeserializeAndValidate(cleanedJson);
            if (parsed is null)
            {
                const string msg = "JSON do Gemini inválido ou fora do schema esperado.";
                logger.LogWarning(msg);
                throw new InvalidOperationException(msg);
            }

            logger.LogInformation("Resposta válida recebida do Gemini.");
            return parsed;
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            var msg = $"Timeout ao chamar Gemini após {RequestTimeout.TotalSeconds}s.";
            logger.LogWarning(msg);
            throw new TimeoutException(msg);
        }
        catch (Exception ex) when (ex is not InvalidOperationException && ex is not TimeoutException)
        {
            logger.LogError(ex, "Falha inesperada ao chamar Gemini.");
            throw;
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
                TipoReuniao = NormalizeTipoReuniao(result.TipoReuniao),
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

    private static string NormalizeTipoReuniao(string? tipoReuniao)
    {
        if (string.IsNullOrWhiteSpace(tipoReuniao))
        {
            return "Desconhecida";
        }

        var normalized = tipoReuniao.Trim().ToLowerInvariant();
        return normalized switch
        {
            "daily" or "diaria" or "diária" => "Diária",
            "planning" or "planejamento" => "Planejamento",
            "status" => "Status",
            "retrospective" or "retrospectiva" => "Retrospectiva",
            "oneonone" or "one-on-one" or "one on one" or "1:1" or "um a um" => "Um a Um",
            "incident" or "incidente" => "Incidente",
            "executive" or "executiva" => "Executiva",
            "unknown" or "desconhecida" => "Desconhecida",
            _ => tipoReuniao.Trim()
        };
    }
}
