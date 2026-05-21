using EasyMeet.Api.Models;
using EasyMeet.Api.Services;

namespace EasyMeet.Tests.Unit;

public sealed class MultiProviderConfigurationTests
{
    private static readonly ProvedorIA[] AllProviders =
    [
        ProvedorIA.Gemini,
        ProvedorIA.Groq,
        ProvedorIA.OpenAI,
        ProvedorIA.Anthropic,
        ProvedorIA.Mistral,
        ProvedorIA.Cohere,
        ProvedorIA.OpenRouter
    ];

    [Fact]
    public async Task ApiKeyManager_DeveSalvarAlterarRemoverChaves()
    {
        var store = new InMemoryApiKeyStore();
        var manager = new ApiKeyManagerService(store, BuildFactory());

        await manager.SalvarOuAtualizarAsync(ProvedorIA.Gemini, "key-1", CancellationToken.None);
        await manager.SalvarOuAtualizarAsync(ProvedorIA.Gemini, "key-2", CancellationToken.None);
        await manager.SalvarOuAtualizarAsync(ProvedorIA.Groq, "groq-1", CancellationToken.None);

        Assert.Equal("key-2", await store.GetApiKeyAsync(ProvedorIA.Gemini, CancellationToken.None));
        Assert.Equal("groq-1", await store.GetApiKeyAsync(ProvedorIA.Groq, CancellationToken.None));

        await manager.RemoverAsync(ProvedorIA.Groq, CancellationToken.None);
        Assert.False(await store.HasApiKeyAsync(ProvedorIA.Groq, CancellationToken.None));
    }

    [Fact]
    public async Task ApiKeyManager_DeveTestarConexao()
    {
        var manager = new ApiKeyManagerService(new InMemoryApiKeyStore(), BuildFactory());
        var ok = await manager.TestarConexaoAsync(ProvedorIA.Gemini, "valid-key", CancellationToken.None);
        Assert.True(ok);
    }

    [Fact]
    public async Task ApiKeyManager_DeveRetornarDetalhe_QuandoTesteConexaoFalha()
    {
        var manager = new ApiKeyManagerService(
            new InMemoryApiKeyStore(),
            new AIProviderFactory([new FailingProvider(ProvedorIA.OpenAI, "OpenAI retornou status 429.")]));

        var result = await manager.TestarConexaoDetalhadoAsync(ProvedorIA.OpenAI, "valid-key", CancellationToken.None);

        Assert.False(result.Sucesso);
        Assert.Contains("429", result.Mensagem);
    }

    [Fact]
    public void ProviderFactory_DeveFalhar_QuandoProvedorInvalido()
    {
        var factory = new AIProviderFactory([new FakeProvider(ProvedorIA.Gemini)]);
        Assert.Throws<InvalidOperationException>(() => factory.GetClient(ProvedorIA.Groq));
    }

    [Fact]
    public async Task ApiKeyManager_StatusDeveCobrirTodosProvedoresSuportados()
    {
        var manager = new ApiKeyManagerService(new InMemoryApiKeyStore(), BuildFactory());
        var status = await manager.ObterStatusAsync(CancellationToken.None);

        Assert.Equal(AllProviders.Length, status.Count);
        foreach (var provedor in AllProviders)
        {
            Assert.Contains(status, x => x.ProvedorIA == provedor);
        }
    }

    private static IAProviderFactory BuildFactory()
    {
        return new AIProviderFactory(AllProviders.Select(x => new FakeProvider(x)));
    }

    private sealed class FakeProvider(ProvedorIA provedor) : IGenerativeAIClient
    {
        public ProvedorIA Provedor => provedor;
        public Task<string> GerarConteudoAsync(
            string prompt,
            string apiKey,
            ConfiguracaoAnaliseIA? configuracaoIA,
            CancellationToken cancellationToken) => Task.FromResult("{}");
        public Task<bool> TestarConexaoAsync(string apiKey, CancellationToken cancellationToken) => Task.FromResult(!string.IsNullOrWhiteSpace(apiKey));
    }

    private sealed class FailingProvider(ProvedorIA provedor, string message) : IGenerativeAIClient
    {
        public ProvedorIA Provedor => provedor;

        public Task<string> GerarConteudoAsync(
            string prompt,
            string apiKey,
            ConfiguracaoAnaliseIA? configuracaoIA,
            CancellationToken cancellationToken)
        {
            throw new InvalidOperationException(message);
        }

        public Task<bool> TestarConexaoAsync(string apiKey, CancellationToken cancellationToken) => Task.FromResult(false);
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
