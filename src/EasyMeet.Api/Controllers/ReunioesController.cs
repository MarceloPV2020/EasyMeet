using EasyMeet.Api.Models;
using EasyMeet.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EasyMeet.Api.Controllers;

/// <summary>
/// Endpoints para analise e resumo de reunioes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class ReunioesController(IAgenteResumoReuniaoService agenteResumoReuniaoService) : ControllerBase
{
    private const int TextoMinimoCaracteres = 20;

    /// <summary>
    /// Recebe o texto de uma reuniao e retorna resumo estruturado gerado por IA.
    /// </summary>
    [HttpPost("resumir")]
    [ProducesResponseType(typeof(ResumoReuniaoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResumirAsync([FromBody] ResumoReuniaoRequest request, CancellationToken cancellationToken)
    {
        var texto = request.Texto?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(texto))
        {
            return BadRequest(new { mensagem = "O campo texto e obrigatorio e nao pode ser vazio." });
        }

        if (texto.Length < TextoMinimoCaracteres)
        {
            return BadRequest(new { mensagem = $"O campo texto deve ter pelo menos {TextoMinimoCaracteres} caracteres." });
        }

        try
        {
            var response = await agenteResumoReuniaoService.ResumirAsync(texto, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (TimeoutException ex)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout, new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            return Problem(
                title: "Falha ao processar o texto da reuniao.",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
