using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EasyMeet.Api.Models;
using EasyMeet.Api.Services;
using Microsoft.Extensions.Options;

namespace EasyMeet.Api.Providers;

[ExcludeFromCodeCoverage]
public sealed class GroqClientService(
    HttpClient httpClient,
    IOptions<GroqSettings> groqSettings,
    ILogger<GroqClientService> logger) : IGenerativeAIClient
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(25);
    public ProvedorIA Provedor => ProvedorIA.Groq;

    public async Task<string> GerarConteudoAsync(
        string prompt,
        string apiKey,
        ConfiguracaoAnaliseIA? configuracaoIA,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Chave de API nao informada.");
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(RequestTimeout);

        var defaultModel = groqSettings.Value.Model;
        var requestedModel = string.IsNullOrWhiteSpace(configuracaoIA?.Modelo) ? defaultModel : configuracaoIA.Modelo.Trim();
        var effectiveConfig = new ConfiguracaoAnaliseIA
        {
            Modelo = requestedModel,
            Temperatura = configuracaoIA?.Temperatura,
            MaxTokens = configuracaoIA?.MaxTokens
        };

        using var request = BuildRequest(prompt, apiKey, defaultModel, effectiveConfig);
        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeoutCts.Token);
        var responseContent = await response.Content.ReadAsStringAsync(timeoutCts.Token);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Groq retornou status {(int)response.StatusCode} usando o modelo '{requestedModel}'. Detalhes: {Shorten(responseContent)}");
        }

        var content = ExtractContent(responseContent);
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException(
                $"Groq retornou resposta sem conteudo textual utilizavel usando o modelo '{requestedModel}'.");
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
            logger.LogWarning(ex, "Falha ao testar conexao com Groq.");
            return false;
        }
    }

    private static HttpRequestMessage BuildRequest(string prompt, string apiKey, string defaultModel, ConfiguracaoAnaliseIA? configuracaoIA)
    {
        var model = string.IsNullOrWhiteSpace(configuracaoIA?.Modelo) ? defaultModel : configuracaoIA.Modelo.Trim();
        var temperature = Math.Clamp(configuracaoIA?.Temperatura ?? 0.2, 0, 2);
        var maxTokens = configuracaoIA?.MaxTokens is > 0 ? Math.Clamp(configuracaoIA.MaxTokens.Value, 64, 8192) : (int?)null;

        var body = new
        {
            model,
            temperature,
            max_tokens = maxTokens,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "openai/v1/chat/completions")
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        return request;
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
