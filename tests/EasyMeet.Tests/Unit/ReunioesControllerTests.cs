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
        // Arrange
        var controller = new ReunioesController(new FakeAgenteResumoReuniaoService());
        var request = new ResumoReuniaoRequest { Texto = "   ", Idioma = "pt-BR" };

        // Act
        var result = await controller.ResumirAsync(request, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ResumirAsync_DeveRetornarBadRequest_QuandoTextoMuitoCurto()
    {
        // Arrange
        var controller = new ReunioesController(new FakeAgenteResumoReuniaoService());
        var request = new ResumoReuniaoRequest { Texto = "curto", Idioma = "pt-BR" };

        // Act
        var result = await controller.ResumirAsync(request, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ResumirAsync_DeveRetornarOk_QuandoEntradaValida()
    {
        // Arrange
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
            Texto = "Este texto possui tamanho suficiente para validação e execução do fluxo completo.",
            Idioma = "pt-BR"
        };

        // Act
        var result = await controller.ResumirAsync(request, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<ResumoReuniaoResponse>(ok.Value);
        Assert.Equal("Resumo de teste", payload.Resumo);
        Assert.Equal("gemini", payload.ModoExecucao);
    }

    private sealed class FakeAgenteResumoReuniaoService : IAgenteResumoReuniaoService
    {
        private readonly ResumoReuniaoResponse _response;

        public FakeAgenteResumoReuniaoService(ResumoReuniaoResponse? response = null)
        {
            _response = response ?? new ResumoReuniaoResponse
            {
                Resumo = "Resumo fake",
                TopicosPrincipais = ["Topico"],
                Acoes = ["Acao"],
                Responsaveis = ["Responsavel"],
                TipoReuniao = "Status",
                NivelConfianca = 0.8,
                GeradoPorIA = true,
                ModoExecucao = "gemini"
            };
        }

        public Task<ResumoReuniaoResponse> ResumirAsync(string texto, string idioma, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_response);
        }
    }
}
