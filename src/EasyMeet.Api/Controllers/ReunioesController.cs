using EasyMeet.Api.Models;
using EasyMeet.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EasyMeet.Api.Controllers;

/// <summary>
/// Endpoints para analise e resumo de reunioes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class ReunioesController(
    IAgenteResumoReuniaoService agenteResumoReuniaoService,
    IReuniaoRepository reuniaoRepository) : ControllerBase
{
    private const int TextoMinimoCaracteres = 20;

    /// <summary>
    /// Lista as reunioes ja analisadas.
    /// </summary>
    [HttpGet]
    [HttpGet("/reunioes")]
    [ProducesResponseType(typeof(IReadOnlyList<Reuniao>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ListarAsync(CancellationToken cancellationToken)
    {
        var reunioes = await reuniaoRepository.ListarAsync(cancellationToken);
        return Ok(reunioes);
    }

    /// <summary>
    /// Recebe o texto de uma reuniao e retorna resumo estruturado gerado por IA.
    /// </summary>
    [HttpPost("resumir")]
    [ProducesResponseType(typeof(ResumoReuniaoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResumirAsync([FromBody] ResumoReuniaoRequest request, CancellationToken cancellationToken)
    {
        var texto = request.Transcricao?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(texto))
        {
            return BadRequest(new { mensagem = "O campo transcricao e obrigatorio e nao pode ser vazio." });
        }

        if (texto.Length < TextoMinimoCaracteres)
        {
            return BadRequest(new { mensagem = $"O campo transcricao deve ter pelo menos {TextoMinimoCaracteres} caracteres." });
        }

        try
        {
            var response = await agenteResumoReuniaoService.ResumirAsync(
                texto,
                request.ProvedorIA,
                request.ConfiguracaoIA,
                cancellationToken);

            await reuniaoRepository.AdicionarAsync(new Reuniao
            {
                Transcricao = texto,
                Resumo = response.Resumo,
                Confianca = double.IsFinite(response.NivelConfianca)
                    ? Convert.ToDecimal(response.NivelConfianca)
                    : 0m
            }, cancellationToken);

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
