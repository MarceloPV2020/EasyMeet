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
        var request = new ResumoReuniaoRequest { Texto = "   " };

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
        var request = new ResumoReuniaoRequest { Texto = "curto" };

        // Act
        var result = await controller.ResumirAsync(request, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ResumirAsync_DeveRetornarOk_QuandoEntradaValida()
    {
        // Arrange
        const string expected = "Resumo de teste";
        var controller = new ReunioesController(new FakeAgenteResumoReuniaoService(expected));
        var request = new ResumoReuniaoRequest
        {
            Texto = "Este texto possui tamanho suficiente para validação e execução do fluxo completo."
        };

        // Act
        var result = await controller.ResumirAsync(request, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<string>(ok.Value);
        Assert.Equal(expected, payload);
    }

    [Fact]
    public async Task ResumirAsync_DeveRetornarBadRequest_QuandoServiceLancaInvalidOperation()
    {
        // Arrange
        var controller = new ReunioesController(new ExceptionAgenteResumoReuniaoService(new InvalidOperationException("Erro simulado")));
        var request = new ResumoReuniaoRequest
        {
            Texto = "Este texto possui tamanho suficiente para validação e execução do fluxo completo."
        };

        // Act
        var result = await controller.ResumirAsync(request, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    private sealed class FakeAgenteResumoReuniaoService(string response = "Resumo fake") : IAgenteResumoReuniaoService
    {
        public Task<string> ResumirAsync(string texto, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(response);
        }
    }

    private sealed class ExceptionAgenteResumoReuniaoService(Exception ex) : IAgenteResumoReuniaoService
    {
        public Task<string> ResumirAsync(string texto, CancellationToken cancellationToken = default)
        {
            throw ex;
        }
    }
}
