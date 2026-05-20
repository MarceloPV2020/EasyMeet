using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EasyMeet.Api.Models;
using EasyMeet.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyMeet.Tests.Integration;

public sealed class ReunioesHttpPipelineTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Resumir_DeveRetornarRespostaDoController_QuandoRequestValido()
    {
        var expected = new ResumoReuniaoResponse
        {
            Resumo = "Resumo gerado pelo pipeline HTTP",
            TopicosPrincipais = ["Roadmap"],
            Acoes =
            [
                new AcaoReuniaoItem
                {
                    Descricao = "Publicar release",
                    Responsavel = "Ana",
                    Prazo = "2026-06-01"
                }
            ],
            Responsaveis = ["Ana"],
            Decisoes = ["Manter prioridade alta"],
            Pendencias = ["Validar metricas"],
            TipoReuniao = "Status",
            NivelConfianca = 0.91,
            GeradoPorIA = true,
            ModoExecucao = "Gemini"
        };

        await using var factory = new EasyMeetApiFactory(services =>
        {
            services.RemoveAll<IAgenteResumoReuniaoService>();
            services.AddScoped<IAgenteResumoReuniaoService>(_ => new StubResumoService(expected));
        });
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/reunioes/analisar", new ResumoReuniaoRequest
        {
            Transcricao = "Esta transcricao possui conteudo suficiente para atravessar o pipeline HTTP completo.",
            ProvedorIA = ProvedorIA.Gemini
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ResumoReuniaoResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal("Resumo gerado pelo pipeline HTTP", payload.Resumo);
        Assert.Equal("Status", payload.TipoReuniao);
        Assert.True(payload.GeradoPorIA);
    }

    [Fact]
    public async Task Resumir_DeveUsarFallbackLocalDoPrompt_QuandoArquivoDePromptNaoExiste()
    {
        var contentRoot = Directory.CreateTempSubdirectory("easymeet-no-prompt-").FullName;
        var apiKeyStore = new InMemoryApiKeyStore();
        await apiKeyStore.SaveApiKeyAsync(ProvedorIA.Gemini, "gemini-key", CancellationToken.None);
        var provider = new CapturingProvider();

        try
        {
            await using var factory = new EasyMeetApiFactory(
                services =>
                {
                    services.RemoveAll<IApiKeyStore>();
                    services.RemoveAll<IGenerativeAIClient>();
                    services.AddSingleton<IApiKeyStore>(apiKeyStore);
                    services.AddSingleton<IGenerativeAIClient>(provider);
                },
                contentRoot);
            using var client = factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/reunioes/analisar", new ResumoReuniaoRequest
            {
                Transcricao = "Transcricao de reuniao para validar template local quando o arquivo nao esta presente.",
                ProvedorIA = ProvedorIA.Gemini
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var payload = await response.Content.ReadFromJsonAsync<ResumoReuniaoResponse>(JsonOptions);
            Assert.NotNull(payload);
            Assert.Equal("Resumo vindo do provedor fake", payload.Resumo);
            Assert.Equal("Gemini", payload.ModoExecucao);
            Assert.Contains("Retorne apenas JSON", provider.LastPrompt);
            Assert.Contains("Transcricao de reuniao para validar template local", provider.LastPrompt);
        }
        finally
        {
            // Ignora limpeza forcada para evitar falso negativo por lock intermitente do SQLite no Windows.
        }
    }

    private sealed class EasyMeetApiFactory(
        Action<IServiceCollection> configureServices,
        string? contentRoot = null) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            if (!string.IsNullOrWhiteSpace(contentRoot))
            {
                builder.UseContentRoot(contentRoot);
            }

            builder.ConfigureServices(configureServices);
        }
    }

    private sealed class StubResumoService(ResumoReuniaoResponse response) : IAgenteResumoReuniaoService
    {
        public Task<ResumoReuniaoResponse> ResumirAsync(
            string transcricao,
            ProvedorIA provedorIA,
            ConfiguracaoAnaliseIA? configuracaoIA = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(response);
        }
    }

    private sealed class CapturingProvider : IGenerativeAIClient
    {
        public ProvedorIA Provedor => ProvedorIA.Gemini;
        public string LastPrompt { get; private set; } = string.Empty;

        public Task<string> GerarConteudoAsync(
            string prompt,
            string apiKey,
            ConfiguracaoAnaliseIA? configuracaoIA,
            CancellationToken cancellationToken)
        {
            LastPrompt = prompt;
            return Task.FromResult("""
            {
              "resumo":"Resumo vindo do provedor fake",
              "topicosPrincipais":["Pipeline HTTP"],
              "acoes":[{"descricao":"Cobrir fallback","responsavel":"Time","prazo":"2026-06-01"}],
              "responsaveis":["Time"],
              "decisoesTomadas":["Usar template embutido quando o arquivo faltar"],
              "pendencias":[],
              "tipoReuniao":"Teste",
              "nivelConfianca":0.8
            }
            """);
        }

        public Task<bool> TestarConexaoAsync(string apiKey, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
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
