using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EasyMeet.Api.Models;
using EasyMeet.Api.Services;
using Microsoft.Extensions.Options;

namespace EasyMeet.Api.Providers;

[ExcludeFromCodeCoverage]
public sealed class OpenRouterClientService(
    HttpClient httpClient,
    IOptions<OpenRouterSettings> openRouterSettings,
    ILogger<OpenRouterClientService> logger) : IGenerativeAIClient
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(30);
    private const int DefaultMaxTokens = 1024;
    public ProvedorIA Provedor => ProvedorIA.OpenRouter;

    public async Task<string> GerarConteudoAsync(string prompt, string apiKey, ConfiguracaoAnaliseIA? configuracaoIA, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Chave de API nao informada.");
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(RequestTimeout);

        var model = string.IsNullOrWhiteSpace(configuracaoIA?.Modelo)
            ? openRouterSettings.Value.Model
            : configuracaoIA.Modelo.Trim();
        var temperature = Math.Clamp(configuracaoIA?.Temperatura ?? 0.2, 0, 2);
        var maxTokens = configuracaoIA?.MaxTokens is > 0
            ? Math.Clamp(configuracaoIA.MaxTokens.Value, 64, 8192)
            : DefaultMaxTokens;

        var response = await SendRequestAsync(
            prompt,
            apiKey,
            model,
            temperature,
            maxTokens,
            timeoutCts.Token);

        var responseContent = await response.Content.ReadAsStringAsync(timeoutCts.Token);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"OpenRouter retornou status {(int)response.StatusCode} usando o modelo '{model}'. Detalhes: {Shorten(responseContent)}");
        }

        var content = ExtractContent(responseContent);
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException($"OpenRouter retornou resposta sem conteudo textual usando o modelo '{model}'.");
        }

        return content;
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
            logger.LogWarning(ex, "Falha ao testar conexao com OpenRouter.");
            return false;
        }
    }

    public async Task<OpenRouterModelosDisponibilidadeResponse> VerificarDisponibilidadeModelosAsync(
        string apiKey,
        IReadOnlyList<string> modelos,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Chave de API nao informada.");
        }

        if (modelos.Count == 0)
        {
            return new OpenRouterModelosDisponibilidadeResponse();
        }

        var normalized = modelos
            .Where(static x => !string.IsNullOrWhiteSpace(x))
            .Select(static x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var disponiveis = new List<string>(capacity: normalized.Length);
        var indisponiveis = new List<string>();

        foreach (var modelo in normalized)
        {
            var path = BuildModelEndpointsPath(modelo);
            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Headers.Add("HTTP-Referer", "https://easymeet.local");
            request.Headers.Add("X-Title", "EasyMeet");

            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                disponiveis.Add(modelo);
            }
            else
            {
                indisponiveis.Add(modelo);
            }
        }

        return new OpenRouterModelosDisponibilidadeResponse
        {
            ModelosDisponiveis = disponiveis,
            ModelosIndisponiveis = indisponiveis
        };
    }


    private static string? ExtractContent(string responseContent)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;
            if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
            {
                return null;
            }

            var first = choices[0];
            if (!first.TryGetProperty("message", out var message))
            {
                return null;
            }

            return message.GetProperty("content").GetString();
        }
        catch
        {
            return null;
        }
    }

    private static string Shorten(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "sem detalhe no body";
        }

        var singleLine = value.Replace('\n', ' ').Replace('\r', ' ').Trim();
        return singleLine.Length <= 220 ? singleLine : singleLine[..220];
    }

    private async Task<HttpResponseMessage> SendRequestAsync(
        string prompt,
        string apiKey,
        string model,
        double temperature,
        int maxTokens,
        CancellationToken cancellationToken)
    {
        var body = new
        {
            model,
            temperature,
            max_tokens = maxTokens,
            response_format = new { type = "json_object" },
            messages = new[] { new { role = "user", content = prompt } }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions")
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {apiKey}");
        request.Headers.Add("HTTP-Referer", "https://easymeet.local");
        request.Headers.Add("X-Title", "EasyMeet");

        return await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }

    private static string BuildModelEndpointsPath(string modelId)
    {
        var segments = modelId.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var encoded = string.Join('/', segments.Select(Uri.EscapeDataString));
        return $"v1/models/{encoded}/endpoints";
    }

}
