using System.Net;
using EasyMeet.Api.Models;
using EasyMeet.Api.Prompts;
using EasyMeet.Api.Services;
using EasyMeet.Tests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace EasyMeet.Tests.Unit;

public sealed class AgenteResumoReuniaoServiceTests
{
    [Fact]
    public async Task ResumirAsync_DeveGerarResumoCorreto_QuandoGeminiRetornaJsonValido()
    {
        var service = CreateServiceWithGeminiPayload(BuildGeminiEnvelope("""
            {
              "resumo": "Resumo final da reuniao de planejamento.",
              "topicosPrincipais": ["Backlog", "Dependencias"],
              "acoes": ["Priorizar historias", "Atualizar cronograma"],
              "responsaveis": ["Ana", "Carlos"],
              "tipoReuniao": "Planning",
              "nivelConfianca": 0.93
            }
            """));

        var result = await service.ResumirAsync("Texto da reuniao com conteudo suficiente para analise de IA.", CancellationToken.None);

        Assert.Equal("Resumo final da reuniao de planejamento.", result.Resumo);
        Assert.Contains("Backlog", result.TopicosPrincipais);
    }

    [Fact]
    public async Task ResumirAsync_DeveLancarExcecao_QuandoGeminiFalhar()
    {
        var service = CreateServiceWithGeminiPayload("{" + "\"erro\":true}", HttpStatusCode.InternalServerError);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ResumirAsync("Texto de reuniao valido para testar erro do Gemini.", CancellationToken.None));
    }

    [Fact]
    public async Task GeminiClientService_DeveMontarRequisicaoCorretaEProcessarResposta()
    {
        var handler = new FakeHttpMessageHandler((_, _) =>
            FakeHttpMessageHandler.JsonResponse(BuildGeminiEnvelope("""
            {
              "resumo": "Resumo teste integracao",
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

        var options = Options.Create(new GeminiSettings { ApiKey = "fake-key" });
        var geminiClientService = new GeminiClientService(client, options, NullLogger<GeminiClientService>.Instance);

        var result = await geminiClientService.TryGerarResumoAsync("Prompt de teste", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Resumo teste integracao", result.Resumo);
        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Contains("gemini-2.5-flash:generateContent", handler.LastRequest.RequestUri!.ToString(), StringComparison.Ordinal);
    }

    private static AgenteResumoReuniaoService CreateServiceWithGeminiPayload(string payload, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handler = new FakeHttpMessageHandler((_, _) => FakeHttpMessageHandler.JsonResponse(payload, statusCode));
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://generativelanguage.googleapis.com/")
        };

        var options = Options.Create(new GeminiSettings { ApiKey = "fake-key" });
        var geminiClientService = new GeminiClientService(client, options, NullLogger<GeminiClientService>.Instance);

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
