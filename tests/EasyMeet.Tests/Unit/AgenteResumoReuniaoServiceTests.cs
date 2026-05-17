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
        var result = await service.ResumirAsync("Texto da reunião com conteúdo suficiente para análise de IA.", "pt-BR", CancellationToken.None);

        // Assert
        Assert.Equal("Resumo final da reunião de planejamento.", result.Resumo);
        Assert.True(result.GeradoPorIA);
        Assert.Equal("gemini", result.ModoExecucao);
    }

    [Fact]
    public async Task ResumirAsync_DeveIdentificarAcoes_QuandoGeminiRetornaAcoes()
    {
        // Arrange
        var service = CreateServiceWithGeminiPayload(BuildGeminiEnvelope("""
            {
              "resumo": "Resumo",
              "topicosPrincipais": ["Tema"],
              "acoes": ["Definir OKRs", "Enviar ata"],
              "responsaveis": ["João"],
              "tipoReuniao": "Status",
              "nivelConfianca": 0.87
            }
            """));

        // Act
        var result = await service.ResumirAsync("Texto de reunião válido com detalhamento de tarefas.", "pt-BR", CancellationToken.None);

        // Assert
        Assert.Contains("Definir OKRs", result.Acoes);
        Assert.Contains("Enviar ata", result.Acoes);
    }

    [Fact]
    public async Task ResumirAsync_DeveIdentificarResponsaveis_QuandoGeminiRetornaResponsaveis()
    {
        // Arrange
        var service = CreateServiceWithGeminiPayload(BuildGeminiEnvelope("""
            {
              "resumo": "Resumo",
              "topicosPrincipais": ["Tema"],
              "acoes": ["A"],
              "responsaveis": ["Marina", "Pedro"],
              "tipoReuniao": "Status",
              "nivelConfianca": 0.9
            }
            """));

        // Act
        var result = await service.ResumirAsync("Texto de reunião válido para identificar responsáveis.", "pt-BR", CancellationToken.None);

        // Assert
        Assert.Contains("Marina", result.Responsaveis);
        Assert.Contains("Pedro", result.Responsaveis);
    }

    [Fact]
    public async Task ResumirAsync_DeveIdentificarTopicosPrincipais_QuandoGeminiRetornaTopicos()
    {
        // Arrange
        var service = CreateServiceWithGeminiPayload(BuildGeminiEnvelope("""
            {
              "resumo": "Resumo",
              "topicosPrincipais": ["Riscos", "Orçamento", "Prazos"],
              "acoes": ["A"],
              "responsaveis": ["R"],
              "tipoReuniao": "Executive",
              "nivelConfianca": 0.88
            }
            """));

        // Act
        var result = await service.ResumirAsync("Texto de reunião válido para identificar tópicos.", "pt-BR", CancellationToken.None);

        // Assert
        Assert.Contains("Riscos", result.TopicosPrincipais);
        Assert.Contains("Orçamento", result.TopicosPrincipais);
        Assert.Contains("Prazos", result.TopicosPrincipais);
    }

    [Fact]
    public async Task ResumirAsync_DeveUsarFallbackLocal_QuandoGeminiFalhar()
    {
        // Arrange
        var service = CreateServiceWithGeminiPayload("{" + "\"erro\":true}", HttpStatusCode.InternalServerError);

        // Act
        var result = await service.ResumirAsync("Texto de reunião válido para fallback local em caso de erro.", "pt-BR", CancellationToken.None);

        // Assert
        Assert.False(result.GeradoPorIA);
        Assert.Equal("fallback_local", result.ModoExecucao);
        Assert.StartsWith("Resumo local (fallback):", result.Resumo);
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
            geminiClientService,
            NullLogger<AgenteResumoReuniaoService>.Instance);
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
