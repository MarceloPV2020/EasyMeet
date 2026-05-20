using EasyMeet.Api.Controllers;
using EasyMeet.Api.Models;
using EasyMeet.Api.Services;
using Microsoft.AspNetCore.Mvc;

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

    private static IAConfigController CreateController(IApiKeyStore store)
    {
        var factory = new AIProviderFactory([
            new FakeProvider(ProvedorIA.Gemini),
            new FakeProvider(ProvedorIA.Groq)
        ]);

        return new IAConfigController(new ApiKeyManagerService(store, factory), factory);
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

