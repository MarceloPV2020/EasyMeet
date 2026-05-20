using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EasyMeet.Api.Models;
using EasyMeet.Api.Services;
using Microsoft.Extensions.Options;

namespace EasyMeet.Api.Providers;

[ExcludeFromCodeCoverage]
public sealed class GeminiClientService(
    HttpClient httpClient,
    IOptions<GeminiSettings> geminiSettings,
    ILogger<GeminiClientService> logger) : IGenerativeAIClient
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(25);
    private const int MaxAttempts = 3;
    public ProvedorIA Provedor => ProvedorIA.Gemini;

    public async Task<string> GerarConteudoAsync(
        string prompt,
        string apiKey,
        ConfiguracaoAnaliseIA? configuracaoIA,
        CancellationToken cancellationToken)
    {
        ValidateApiKey(apiKey);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(RequestTimeout);

        var requestedModel = ResolveModelName(configuracaoIA?.Modelo);
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            using var request = BuildRequest(prompt, apiKey, requestedModel, configuracaoIA);
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeoutCts.Token);
            var responseContent = await response.Content.ReadAsStringAsync(timeoutCts.Token);

            if (response.IsSuccessStatusCode)
            {
                var candidateText = ExtractCandidateText(responseContent);
                if (!string.IsNullOrWhiteSpace(candidateText))
                {
                    return candidateText;
                }

                throw new InvalidOperationException(
                    $"Gemini retornou resposta sem conteudo textual usando o modelo '{requestedModel}'.");
            }

            var shouldRetry = response.StatusCode is System.Net.HttpStatusCode.ServiceUnavailable
                or System.Net.HttpStatusCode.TooManyRequests
                or System.Net.HttpStatusCode.GatewayTimeout;

            if (shouldRetry && attempt < MaxAttempts)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(600 * attempt), timeoutCts.Token);
                continue;
            }

            var snippet = Shorten(responseContent);
            throw new InvalidOperationException(
                $"Gemini retornou status {(int)response.StatusCode} usando o modelo '{requestedModel}'. Detalhes: {snippet}");
        }

        throw new InvalidOperationException(
            $"Gemini nao concluiu a requisicao apos {MaxAttempts} tentativas usando o modelo '{requestedModel}'.");
    }

    public async Task<bool> TestarConexaoAsync(string apiKey, CancellationToken cancellationToken)
    {
        try
        {
            _ = await GerarConteudoAsync("Responda apenas OK", apiKey, null, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao testar conexao com Gemini.");
            return false;
        }
    }

    private static void ValidateApiKey(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Chave de API nao informada.");
        }
    }

    private string ResolveModelName(string? requestedModel)
    {
        var model = string.IsNullOrWhiteSpace(requestedModel) ? geminiSettings.Value.Model?.Trim() : requestedModel.Trim();
        return string.IsNullOrWhiteSpace(model) ? "gemini-2.5-flash" : model;
    }

    private static string NormalizeModelName(string? modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            return string.Empty;
        }

        var normalized = modelName.Trim();
        return normalized.StartsWith("models/", StringComparison.OrdinalIgnoreCase)
            ? normalized["models/".Length..]
            : normalized;
    }

    private static HttpRequestMessage BuildRequest(string prompt, string apiKey, string modelName, ConfiguracaoAnaliseIA? configuracaoIA)
    {
        var normalizedModelName = NormalizeModelName(modelName);
        var uri = $"v1beta/models/{normalizedModelName}:generateContent";
        var temperature = Math.Clamp(configuracaoIA?.Temperatura ?? 0.2, 0, 2);
        var maxTokens = configuracaoIA?.MaxTokens is > 0 ? Math.Clamp(configuracaoIA.MaxTokens.Value, 64, 8192) : (int?)null;

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
                temperature,
                maxOutputTokens = maxTokens,
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

    private static string Shorten(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "sem detalhe no body";
        }

        var singleLine = value.Replace('\n', ' ').Replace('\r', ' ').Trim();
        return singleLine.Length <= 180 ? singleLine : singleLine[..180];
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

            return parts[0].GetProperty("text").GetString();
        }
        catch
        {
            return null;
        }
    }
}
