using EasyMeet.Api.Models;
using EasyMeet.Api.Prompts;
using EasyMeet.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;

namespace EasyMeet.Tests.Unit;

public sealed class AgenteResumoReuniaoServiceTests
{
    [Fact]
    public async Task ResumirAsync_DeveBloquear_QuandoNaoExisteChave()
    {
        var service = CreateService(
            new InMemoryApiKeyStore(),
            new[] { new FakeProvider(ProvedorIA.Gemini, "{}") });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ResumirAsync("Transcricao valida para teste da regra.", ProvedorIA.Gemini, cancellationToken: CancellationToken.None));

        Assert.Contains("Chave de API nao configurada", ex.Message);
    }

    [Fact]
    public async Task ResumirAsync_DeveSelecionarProviderCorreto()
    {
        var store = new InMemoryApiKeyStore();
        await store.SaveApiKeyAsync(ProvedorIA.Groq, "groq-key", CancellationToken.None);

        var groqPayload = """
        {
          "resumo":"Resumo via Groq",
          "topicosPrincipais":["Topico"],
          "acoes":[{"descricao":"Acao","responsavel":"Ana","prazo":"2026-06-01"}],
          "responsaveis":["Ana"],
          "decisoesTomadas":["Decisao"],
          "pendencias":["Pendencia"],
          "tipoReuniao":"Status",
          "nivelConfianca":0.9
        }
        """;

        var provider = new FakeProvider(ProvedorIA.Groq, groqPayload);
        var service = CreateService(store, [provider]);

        var result = await service.ResumirAsync("Transcricao valida para roteamento.", ProvedorIA.Groq, cancellationToken: CancellationToken.None);

        Assert.Equal("Resumo via Groq", result.Resumo);
        Assert.Equal(1, provider.Chamadas);
        Assert.Equal("Groq", result.ModoExecucao);
        Assert.Equal("llama-3.1-8b-instant", result.ModeloIA);
    }

    [Fact]
    public async Task ResumirAsync_DeveMapearRespostaEstruturada()
    {
        var store = new InMemoryApiKeyStore();
        await store.SaveApiKeyAsync(ProvedorIA.Gemini, "gemini-key", CancellationToken.None);

        var payload = """
        {
          "resumo":"Resumo final",
          "topicosPrincipais":["Planejamento"],
          "acoes":[{"descricao":"Atualizar backlog","responsavel":"Carlos","prazo":"2026-06-10"}],
          "responsaveis":["Carlos"],
          "decisoesTomadas":["Prioridade alta para sprint"],
          "pendencias":["Definir prazo final"],
          "tipoReuniao":"Planning",
          "nivelConfianca":0.88
        }
        """;

        var service = CreateService(store, [new FakeProvider(ProvedorIA.Gemini, payload)]);
        var result = await service.ResumirAsync(
            "Transcricao completa de reuniao.",
            ProvedorIA.Gemini,
            new ConfiguracaoAnaliseIA { Modelo = "gemini-2.0-flash" },
            CancellationToken.None);

        Assert.Equal("Resumo final", result.Resumo);
        Assert.Single(result.Decisoes);
        Assert.Single(result.Pendencias);
        Assert.Equal("gemini-2.0-flash", result.ModeloIA);
    }

    private static AgenteResumoReuniaoService CreateService(IApiKeyStore keyStore, IEnumerable<IGenerativeAIClient> providers)
    {
        var environment = new FakeWebHostEnvironment
        {
            ContentRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../src/EasyMeet.Api"))
        };

        return new AgenteResumoReuniaoService(
            new PromptResumoReuniaoBuilder(NullLogger<PromptResumoReuniaoBuilder>.Instance, environment),
            keyStore,
            new AIProviderFactory(providers),
            new MeetingAnalysisParser());
    }

    private sealed class FakeProvider(ProvedorIA provedor, string retorno) : IGenerativeAIClient
    {
        public ProvedorIA Provedor => provedor;
        public int Chamadas { get; private set; }

        public Task<string> GerarConteudoAsync(
            string prompt,
            string apiKey,
            ConfiguracaoAnaliseIA? configuracaoIA,
            CancellationToken cancellationToken)
        {
            Chamadas++;
            return Task.FromResult(retorno);
        }

        public Task<bool> TestarConexaoAsync(string apiKey, CancellationToken cancellationToken) => Task.FromResult(true);
    }

    private sealed class InMemoryApiKeyStore : IApiKeyStore
    {
        private readonly Dictionary<ProvedorIA, string> _keys = new();

        public Task SaveApiKeyAsync(ProvedorIA provedor, string apiKey, CancellationToken cancellationToken)
        {
            _keys[provedor] = apiKey;
            return Task.CompletedTask;
        }

        public Task<string?> GetApiKeyAsync(ProvedorIA provedor, CancellationToken cancellationToken)
        {
            _keys.TryGetValue(provedor, out var value);
            return Task.FromResult<string?>(value);
        }

        public Task DeleteApiKeyAsync(ProvedorIA provedor, CancellationToken cancellationToken)
        {
            _keys.Remove(provedor);
            return Task.CompletedTask;
        }

        public Task<bool> HasApiKeyAsync(ProvedorIA provedor, CancellationToken cancellationToken)
        {
            return Task.FromResult(_keys.ContainsKey(provedor));
        }
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
