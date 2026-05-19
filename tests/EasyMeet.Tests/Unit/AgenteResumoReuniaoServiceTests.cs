using System.Net;
using EasyMeet.Api.Models;
using EasyMeet.Api.Prompts;
using EasyMeet.Api.Services;
using EasyMeet.Tests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;

namespace EasyMeet.Tests.Unit;

public sealed class AgenteResumoReuniaoServiceTests
{
    [Fact]
    public async Task ResumirAsync_DeveGerarResumoCorreto_QuandoGeminiRetornaJsonValido()
    {
        // Arrange
        var service = CreateServiceWithGeminiPayload(BuildGeminiEnvelope("""
            {
              "resumo": "Resumo final da reunião de planejamento.",
              "topicosPrincipais": ["Backlog", "Dependências"],
              "acoes": ["Priorizar histórias", "Atualizar cronograma"],
              "responsaveis": ["Ana", "Carlos"],
              "tipoReuniao": "Planning",
              "nivelConfianca": 0.93
            }
            """));

        // Act
        var result = await service.ResumirAsync("Texto da reunião com conteúdo suficiente para análise de IA.", CancellationToken.None);

        // Assert
        Assert.Equal("Resumo final da reunião de planejamento.", result);
    }

    [Fact]
    public async Task ResumirAsync_DeveLancarExcecao_QuandoGeminiFalhar()
    {
        // Arrange
        var service = CreateServiceWithGeminiPayload("{" + "\"erro\":true}", HttpStatusCode.InternalServerError);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            service.ResumirAsync("Texto de reunião válido para testar erro do Gemini.", CancellationToken.None));
    }

    [Fact]
    public async Task GeminiClientService_DeveMontarRequisicaoCorretaEProcessarResposta()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler((_, _) =>
            FakeHttpMessageHandler.JsonResponse(BuildGeminiEnvelope("""
            {
              "resumo": "Resumo teste integração",
              "topicosPrincipais": ["Topico 1"],
              "acoes": ["Acao 1"],
              "responsaveis": ["Resp 1"],
              "tipoReuniao": "Daily",
              "nivelConfianca": 0.81
            }
            """)));

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://generativelanguage.googleapis.com/")
        };

        var geminiClientService = new GeminiClientService(client, NullLogger<GeminiClientService>.Instance);

        Environment.SetEnvironmentVariable("GEMINI_API_KEY", "fake-key");

        // Act
        var result = await geminiClientService.TryGerarResumoAsync("Prompt de teste", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Resumo teste integração", result.Resumo);
        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Contains("gemini-2.5-flash:generateContent", handler.LastRequest.RequestUri!.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task GeminiClientService_DeveLancarExcecao_QuandoApiKeyNaoConfigurada()
    {
        // Arrange
        var client = new HttpClient();
        var geminiClientService = new GeminiClientService(client, NullLogger<GeminiClientService>.Instance);
        Environment.SetEnvironmentVariable("GEMINI_API_KEY", "");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            geminiClientService.TryGerarResumoAsync("Prompt de teste", CancellationToken.None));
        
        Assert.Contains("GEMINI_API_KEY não configurada", ex.Message);
    }

    private static AgenteResumoReuniaoService CreateServiceWithGeminiPayload(string payload, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        Environment.SetEnvironmentVariable("GEMINI_API_KEY", "fake-key");

        var handler = new FakeHttpMessageHandler((_, _) => FakeHttpMessageHandler.JsonResponse(payload, statusCode));
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://generativelanguage.googleapis.com/")
        };

        var geminiClientService = new GeminiClientService(client, NullLogger<GeminiClientService>.Instance);
        var environment = new FakeWebHostEnvironment
        {
            ContentRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../src/EasyMeet.Api"))
        };

        var promptBuilder = new PromptResumoReuniaoBuilder(NullLogger<PromptResumoReuniaoBuilder>.Instance, environment);

        return new AgenteResumoReuniaoService(
            promptBuilder,
            geminiClientService);
    }

    private static string BuildGeminiEnvelope(string innerJson)
    {
        return $$"""
        {
          "candidates": [
            {
              "content": {
                "parts": [
                  {
                    "text": {{System.Text.Json.JsonSerializer.Serialize(innerJson)}}
                  }
                ]
              }
            }
          ]
        }
        """;
    }

    private sealed class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "EasyMeet.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
