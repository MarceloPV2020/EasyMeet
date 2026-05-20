using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EasyMeet.Api.Models;
using EasyMeet.Api.Services;
using Microsoft.Extensions.Options;

namespace EasyMeet.Api.Providers;

[ExcludeFromCodeCoverage]
public sealed class AzureOpenAIClientService(
    HttpClient httpClient,
    IOptions<AzureOpenAISettings> azureSettings,
    ILogger<AzureOpenAIClientService> logger) : IGenerativeAIClient
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(30);
    public ProvedorIA Provedor => ProvedorIA.AzureOpenAI;

    public async Task<string> GerarConteudoAsync(string prompt, string apiKey, ConfiguracaoAnaliseIA? configuracaoIA, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Chave de API nao informada.");
        }

        var endpoint = azureSettings.Value.Endpoint?.Trim();
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new InvalidOperationException("Azure OpenAI endpoint nao configurado.");
        }

        if (httpClient.BaseAddress is null)
        {
            httpClient.BaseAddress = new Uri(endpoint);
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(RequestTimeout);

        var deployment = string.IsNullOrWhiteSpace(configuracaoIA?.Modelo) ? azureSettings.Value.Deployment : configuracaoIA.Modelo.Trim();
        var apiVersion = string.IsNullOrWhiteSpace(azureSettings.Value.ApiVersion) ? "2024-10-21" : azureSettings.Value.ApiVersion;
        var temperature = Math.Clamp(configuracaoIA?.Temperatura ?? 0.2, 0, 2);
        var maxTokens = configuracaoIA?.MaxTokens is > 0 ? Math.Clamp(configuracaoIA.MaxTokens.Value, 64, 8192) : (int?)null;

        var body = new
        {
            temperature,
            max_tokens = maxTokens,
            messages = new[] { new { role = "user", content = prompt } }
        };

        var uri = $"openai/deployments/{deployment}/chat/completions?api-version={Uri.EscapeDataString(apiVersion)}";
        using var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("api-key", apiKey);

        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeoutCts.Token);
        var responseContent = await response.Content.ReadAsStringAsync(timeoutCts.Token);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"AzureOpenAI retornou status {(int)response.StatusCode} usando o deployment '{deployment}'. Detalhes: {Shorten(responseContent)}");
        }

        var content = ExtractContent(responseContent);
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException($"AzureOpenAI retornou resposta sem conteudo textual usando o deployment '{deployment}'.");
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
            logger.LogWarning(ex, "Falha ao testar conexao com AzureOpenAI.");
            return false;
        }
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
}
