using EasyMeet.Api.Controllers;
using EasyMeet.Api.Models;
using EasyMeet.Api.Providers;
using EasyMeet.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace EasyMeet.Tests.Unit;

public sealed class IAConfigControllerTests
{
    [Fact]
    public void ListarProvedores_DeveRetornarProvedoresDisponiveis()
    {
        var controller = CreateController(new InMemoryApiKeyStore());

        var result = controller.ListarProvedores();

        var ok = Assert.IsType<OkObjectResult>(result);
        var providers = Assert.IsType<ProvedorIAResponse[]>(ok.Value);
        Assert.Contains(providers, x => x.ProvedorIA == ProvedorIA.Gemini);
        Assert.Contains(providers, x => x.ProvedorIA == ProvedorIA.Groq);
    }

    [Fact]
    public async Task ObterStatus_DeveIndicarCredencialConfigurada()
    {
        var store = new InMemoryApiKeyStore();
        await store.SaveApiKeyAsync(ProvedorIA.Groq, "groq-key", CancellationToken.None);
        var controller = CreateController(store);

        var result = await controller.ObterStatus(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var status = Assert.IsAssignableFrom<IReadOnlyList<ApiKeyStatusResponse>>(ok.Value);
        Assert.Contains(status, x => x.ProvedorIA == ProvedorIA.Groq && x.Configurado);
    }

    [Fact]
    public async Task SalvarERemoverCredencial_DeveAtualizarStore()
    {
        var store = new InMemoryApiKeyStore();
        var controller = CreateController(store);

        await controller.SalvarCredencial(
            new SalvarApiKeyRequest { ProvedorIA = ProvedorIA.Gemini, ApiKey = "gemini-key" },
            CancellationToken.None);

        Assert.True(await store.HasApiKeyAsync(ProvedorIA.Gemini, CancellationToken.None));

        await controller.RemoverCredencial(ProvedorIA.Gemini, CancellationToken.None);

        Assert.False(await store.HasApiKeyAsync(ProvedorIA.Gemini, CancellationToken.None));
    }

    [Fact]
    public async Task TestarCredencial_DeveRetornarResultadoDetalhado()
    {
        var controller = CreateController(new InMemoryApiKeyStore());

        var result = await controller.TestarCredencial(
            new TestarApiKeyRequest { ProvedorIA = ProvedorIA.Gemini, ApiKey = "valid-key" },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<ResultadoTesteConexaoResponse>(ok.Value);
        Assert.True(payload.Sucesso);
    }

    [Fact]
    public async Task VerificarModelosOpenRouterAsync_DeveRetornarBadRequest_QuandoListaVazia()
    {
        var controller = CreateController(new InMemoryApiKeyStore());

        var result = await controller.VerificarModelosOpenRouterAsync(
            new OpenRouterModelosDisponibilidadeRequest { Modelos = [] },
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task VerificarModelosOpenRouterAsync_DeveRetornarBadRequest_QuandoApiKeyNaoConfigurada()
    {
        var controller = CreateController(new InMemoryApiKeyStore());

        var result = await controller.VerificarModelosOpenRouterAsync(
            new OpenRouterModelosDisponibilidadeRequest { Modelos = ["anthropic/claude-3.5-haiku"] },
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task VerificarModelosOpenRouterAsync_DeveRetornarModelosDisponiveis_QuandoApiKeyConfigurada()
    {
        var store = new InMemoryApiKeyStore();
        await store.SaveApiKeyAsync(ProvedorIA.OpenRouter, "sk-or-v1-test-key", CancellationToken.None);
        var controller = CreateController(store);

        var result = await controller.VerificarModelosOpenRouterAsync(
            new OpenRouterModelosDisponibilidadeRequest
            {
                Modelos = ["anthropic/claude-3.5-haiku", "openai/gpt-4o-mini"]
            },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<OpenRouterModelosDisponibilidadeResponse>(ok.Value);
        Assert.Contains("anthropic/claude-3.5-haiku", payload.ModelosDisponiveis);
        Assert.Contains("openai/gpt-4o-mini", payload.ModelosDisponiveis);
        Assert.Empty(payload.ModelosIndisponiveis);
    }

    private static IAConfigController CreateController(IApiKeyStore store)
    {
        var factory = new AIProviderFactory([
            new FakeProvider(ProvedorIA.Gemini),
            new FakeProvider(ProvedorIA.Groq)
        ]);
        var openRouterClient = new OpenRouterClientService(
            new HttpClient(new FakeHttpMessageHandler(_ => new HttpResponseMessage(System.Net.HttpStatusCode.OK)))
            {
                BaseAddress = new Uri("https://openrouter.ai/api/")
            },
            Options.Create(new OpenRouterSettings()),
            NullLogger<OpenRouterClientService>.Instance);

        return new IAConfigController(new ApiKeyManagerService(store, factory), factory, store, openRouterClient);
    }

    private sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(handler(request));
        }
    }

    private sealed class FakeProvider(ProvedorIA provedor) : IGenerativeAIClient
    {
        public ProvedorIA Provedor => provedor;

        public Task<string> GerarConteudoAsync(
            string prompt,
            string apiKey,
            ConfiguracaoAnaliseIA? configuracaoIA,
            CancellationToken cancellationToken)
        {
            return Task.FromResult("""{"ok":true}""");
        }

        public Task<bool> TestarConexaoAsync(string apiKey, CancellationToken cancellationToken)
        {
            return Task.FromResult(!string.IsNullOrWhiteSpace(apiKey));
        }
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
}

