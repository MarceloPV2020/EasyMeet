using EasyMeet.Api.Controllers;
using EasyMeet.Api.Models;
using EasyMeet.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EasyMeet.Tests.Unit;

public sealed class ReunioesControllerTests
{
    [Fact]
    public async Task ResumirAsync_DeveRetornarBadRequest_QuandoTextoVazio()
    {
        var controller = new ReunioesController(new FakeAgenteResumoReuniaoService());
        var request = new ResumoReuniaoRequest { Texto = "   " };

        var result = await controller.ResumirAsync(request, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ResumirAsync_DeveRetornarBadRequest_QuandoTextoMuitoCurto()
    {
        var controller = new ReunioesController(new FakeAgenteResumoReuniaoService());
        var request = new ResumoReuniaoRequest { Texto = "curto" };

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
            Acoes = ["Acao A"],
            Responsaveis = ["Pessoa A"],
            TipoReuniao = "Status",
            NivelConfianca = 0.91,
            GeradoPorIA = true,
            ModoExecucao = "gemini"
        };

        var controller = new ReunioesController(new FakeAgenteResumoReuniaoService(expected));
        var request = new ResumoReuniaoRequest
        {
            Texto = "Este texto possui tamanho suficiente para validacao e execucao do fluxo completo."
        };

        var result = await controller.ResumirAsync(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<ResumoReuniaoResponse>(ok.Value);
        Assert.Equal("Resumo de teste", payload.Resumo);
        Assert.Equal("gemini", payload.ModoExecucao);
    }

    [Fact]
    public async Task ResumirAsync_DeveRetornarBadRequest_QuandoServiceLancaInvalidOperation()
    {
        var controller = new ReunioesController(new ExceptionAgenteResumoReuniaoService(new InvalidOperationException("Erro simulado")));
        var request = new ResumoReuniaoRequest
        {
            Texto = "Este texto possui tamanho suficiente para validacao e execucao do fluxo completo."
        };

        var result = await controller.ResumirAsync(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    private sealed class FakeAgenteResumoReuniaoService(ResumoReuniaoResponse? response = null) : IAgenteResumoReuniaoService
    {
        public Task<ResumoReuniaoResponse> ResumirAsync(string texto, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(response ?? new ResumoReuniaoResponse
            {
                Resumo = "Resumo fake",
                TopicosPrincipais = ["Topico"],
                Acoes = ["Acao"],
                Responsaveis = ["Responsavel"],
                TipoReuniao = "Status",
                NivelConfianca = 0.8,
                GeradoPorIA = true,
                ModoExecucao = "gemini"
            });
        }
    }

    private sealed class ExceptionAgenteResumoReuniaoService(Exception ex) : IAgenteResumoReuniaoService
    {
        public Task<ResumoReuniaoResponse> ResumirAsync(string texto, CancellationToken cancellationToken = default)
        {
            throw ex;
        }
    }
}
