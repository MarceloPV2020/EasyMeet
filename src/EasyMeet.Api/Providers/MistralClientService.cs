using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EasyMeet.Api.Models;
using EasyMeet.Api.Services;
using Microsoft.Extensions.Options;

namespace EasyMeet.Api.Providers;

public sealed class MistralClientService(
    HttpClient httpClient,
    IOptions<MistralSettings> mistralSettings,
    ILogger<MistralClientService> logger) : IGenerativeAIClient
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(30);
    public ProvedorIA Provedor => ProvedorIA.Mistral;

    public async Task<string> GerarConteudoAsync(string prompt, string apiKey, ConfiguracaoAnaliseIA? configuracaoIA, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Chave de API nao informada.");
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(RequestTimeout);

        var model = string.IsNullOrWhiteSpace(configuracaoIA?.Modelo) ? mistralSettings.Value.Model : configuracaoIA.Modelo.Trim();
        var temperature = Math.Clamp(configuracaoIA?.Temperatura ?? 0.2, 0, 2);
        var maxTokens = configuracaoIA?.MaxTokens is > 0 ? Math.Clamp(configuracaoIA.MaxTokens.Value, 64, 8192) : (int?)null;

        var body = new
        {
            model,
            temperature,
            max_tokens = maxTokens,
            messages = new[] { new { role = "user", content = prompt } }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions")
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeoutCts.Token);
        var responseContent = await response.Content.ReadAsStringAsync(timeoutCts.Token);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Mistral retornou status {(int)response.StatusCode} usando o modelo '{model}'. Detalhes: {Shorten(responseContent)}");
        }

        var content = ExtractContent(responseContent);
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException($"Mistral retornou resposta sem conteudo textual usando o modelo '{model}'.");
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
            logger.LogWarning(ex, "Falha ao testar conexao com Mistral.");
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
