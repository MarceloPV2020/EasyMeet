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
        var controller = new ReunioesController(new FakeAgenteResumoReuniaoService());
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

        var controller = new ReunioesController(new FakeAgenteResumoReuniaoService(expected));
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
    public async Task ResumirAsync_DeveRetornarBadRequest_QuandoServiceLancaInvalidOperation()
    {
        var controller = new ReunioesController(new ExceptionAgenteResumoReuniaoService(new InvalidOperationException("Erro simulado")));
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
}
