using EasyMeet.Api.Controllers;
using EasyMeet.Api.Models;
using EasyMeet.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EasyMeet.Tests.Unit;

public sealed class ReunioesControllerTests
{
    [Fact]
    public async Task ResumirAsync_DeveRetornarBadRequest_QuandoTranscricaoVazia()
    {
        var controller = new ReunioesController(
            new FakeAgenteResumoReuniaoService(),
            new InMemoryReuniaoRepository());
        var request = new ResumoReuniaoRequest { Transcricao = "   ", ProvedorIA = ProvedorIA.Gemini };

        var result = await controller.ResumirAsync(request, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ResumirAsync_DeveRetornarOk_QuandoEntradaValida()
    {
        var expected = new ResumoReuniaoResponse
        {
            Resumo = "Resumo de teste",
            TopicosPrincipais = ["Topico A"],
            Acoes =
            [
                new AcaoReuniaoItem
                {
                    Descricao = "Acao A",
                    Responsavel = "Pessoa A",
                    Prazo = "2026-05-30"
                }
            ],
            Responsaveis = ["Pessoa A"],
            Decisoes = ["Decisao A"],
            Pendencias = ["Pendencia A"],
            TipoReuniao = "Status",
            NivelConfianca = 0.9,
            GeradoPorIA = true,
            ModoExecucao = "Gemini"
        };

        var controller = new ReunioesController(
            new FakeAgenteResumoReuniaoService(expected),
            new InMemoryReuniaoRepository());
        var request = new ResumoReuniaoRequest
        {
            Transcricao = "Este texto possui tamanho suficiente para validar o fluxo completo da API.",
            ProvedorIA = ProvedorIA.Groq
        };

        var result = await controller.ResumirAsync(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<ResumoReuniaoResponse>(ok.Value);
        Assert.Equal("Resumo de teste", payload.Resumo);
    }

    [Fact]
    public async Task ResumirAsync_DeveSalvarReuniao_QuandoEntradaValida()
    {
        var repository = new InMemoryReuniaoRepository();
        var controller = new ReunioesController(new FakeAgenteResumoReuniaoService(new ResumoReuniaoResponse
        {
            Resumo = "Resumo persistido",
            TopicosPrincipais = ["Topico persistido"],
            Acoes =
            [
                new AcaoReuniaoItem
                {
                    Descricao = "Acao persistida",
                    Responsavel = "Pessoa persistida",
                    Prazo = "2026-06-02"
                }
            ],
            Responsaveis = ["Pessoa persistida"],
            Decisoes = ["Decisao persistida"],
            Pendencias = ["Pendencia persistida"],
            DataReuniao = "2026-05-20",
            TipoReuniao = "Status",
            NivelConfianca = 0.82,
            GeradoPorIA = true,
            ModoExecucao = "Gemini"
        }), repository);
        var request = new ResumoReuniaoRequest
        {
            Transcricao = "Este texto possui tamanho suficiente para ser salvo no historico.",
            ProvedorIA = ProvedorIA.Gemini
        };

        await controller.ResumirAsync(request, CancellationToken.None);

        var reuniao = Assert.Single(repository.Reunioes);
        Assert.Equal(request.Transcricao, reuniao.Transcricao);
        Assert.Equal("Resumo persistido", reuniao.Resumo);
        Assert.Equal(["Topico persistido"], reuniao.TopicosPrincipais);
        var acao = Assert.Single(reuniao.Acoes);
        Assert.Equal("Acao persistida", acao.Descricao);
        Assert.Equal("Pessoa persistida", acao.Responsavel);
        Assert.Equal("2026-06-02", acao.Prazo);
        Assert.Equal(["Pessoa persistida"], reuniao.Responsaveis);
        Assert.Equal(["Decisao persistida"], reuniao.Decisoes);
        Assert.Equal(["Pendencia persistida"], reuniao.Pendencias);
        Assert.Equal("2026-05-20", reuniao.DataReuniao);
        Assert.Equal("Status", reuniao.TipoReuniao);
        Assert.Equal(0.82m, reuniao.Confianca);
        Assert.True(reuniao.GeradoPorIA);
        Assert.Equal("Gemini", reuniao.ModoExecucao);
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarReunioes()
    {
        var repository = new InMemoryReuniaoRepository();
        await repository.AdicionarAsync(new Reuniao
        {
            Transcricao = "Transcricao salva",
            Resumo = "Resumo salvo",
            Confianca = 0.7m
        });
        var controller = new ReunioesController(new FakeAgenteResumoReuniaoService(), repository);

        var result = await controller.ListarAsync(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsAssignableFrom<IReadOnlyList<Reuniao>>(ok.Value);
        Assert.Single(payload);
    }

    [Fact]
    public async Task RemoverAsync_DeveRetornarNoContent_QuandoRegistroExiste()
    {
        var repository = new InMemoryReuniaoRepository();
        var id = await repository.AdicionarAsync(new Reuniao
        {
            Transcricao = "Transcricao salva",
            Resumo = "Resumo salvo",
            Confianca = 0.7m
        });
        var controller = new ReunioesController(new FakeAgenteResumoReuniaoService(), repository);

        var result = await controller.RemoverAsync(id, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(repository.Reunioes);
    }

    [Fact]
    public async Task RemoverAsync_DeveRetornarNotFound_QuandoRegistroNaoExiste()
    {
        var controller = new ReunioesController(
            new FakeAgenteResumoReuniaoService(),
            new InMemoryReuniaoRepository());

        var result = await controller.RemoverAsync(999, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task RemoverAsync_DeveRetornarBadRequest_QuandoIdInvalido()
    {
        var controller = new ReunioesController(
            new FakeAgenteResumoReuniaoService(),
            new InMemoryReuniaoRepository());

        var result = await controller.RemoverAsync(0, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ResumirAsync_DeveRetornarBadRequest_QuandoServiceLancaInvalidOperation()
    {
        var controller = new ReunioesController(
            new ExceptionAgenteResumoReuniaoService(new InvalidOperationException("Erro simulado")),
            new InMemoryReuniaoRepository());
        var request = new ResumoReuniaoRequest
        {
            Transcricao = "Este texto possui tamanho suficiente para validar o fluxo completo da API.",
            ProvedorIA = ProvedorIA.Gemini
        };

        var result = await controller.ResumirAsync(request, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    private sealed class FakeAgenteResumoReuniaoService(ResumoReuniaoResponse? response = null) : IAgenteResumoReuniaoService
    {
        public Task<ResumoReuniaoResponse> ResumirAsync(
            string transcricao,
            ProvedorIA provedorIA,
            ConfiguracaoAnaliseIA? configuracaoIA = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(response ?? new ResumoReuniaoResponse
            {
                Resumo = "Resumo fake",
                TopicosPrincipais = [],
                Acoes = [],
                Responsaveis = [],
                Decisoes = [],
                Pendencias = [],
                TipoReuniao = "Desconhecida",
                NivelConfianca = 0.5,
                GeradoPorIA = true,
                ModoExecucao = provedorIA.ToString()
            });
        }
    }

    private sealed class ExceptionAgenteResumoReuniaoService(Exception ex) : IAgenteResumoReuniaoService
    {
        public Task<ResumoReuniaoResponse> ResumirAsync(
            string transcricao,
            ProvedorIA provedorIA,
            ConfiguracaoAnaliseIA? configuracaoIA = null,
            CancellationToken cancellationToken = default)
        {
            throw ex;
        }
    }

    private sealed class InMemoryReuniaoRepository : IReuniaoRepository
    {
        public List<Reuniao> Reunioes { get; } = [];

        public Task InicializarAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<int> AdicionarAsync(Reuniao reuniao, CancellationToken cancellationToken = default)
        {
            var id = Reunioes.Count + 1;
            Reunioes.Add(new Reuniao
            {
                Id = id,
                Transcricao = reuniao.Transcricao,
                Resumo = reuniao.Resumo,
                TopicosPrincipais = reuniao.TopicosPrincipais,
                Acoes = reuniao.Acoes,
                Responsaveis = reuniao.Responsaveis,
                Decisoes = reuniao.Decisoes,
                Pendencias = reuniao.Pendencias,
                DataReuniao = reuniao.DataReuniao,
                TipoReuniao = reuniao.TipoReuniao,
                Confianca = reuniao.Confianca,
                GeradoPorIA = reuniao.GeradoPorIA,
                ModoExecucao = reuniao.ModoExecucao
            });
            return Task.FromResult(id);
        }

        public Task<IReadOnlyList<Reuniao>> ListarAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Reuniao>>(Reunioes);
        }

        public Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
        {
            var removed = Reunioes.RemoveAll(reuniao => reuniao.Id == id) > 0;
            return Task.FromResult(removed);
        }
    }
}
