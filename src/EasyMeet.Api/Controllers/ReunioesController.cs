using EasyMeet.Api.Models;
using EasyMeet.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EasyMeet.Api.Controllers;

/// <summary>
/// Endpoints para analise de reunioes.
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
    /// Remove uma reuniao do historico local.
    /// </summary>
    [HttpDelete("{id:int}")]
    [HttpDelete("/reunioes/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoverAsync([FromRoute] int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new { mensagem = "O identificador da reuniao deve ser maior que zero." });
        }

        var removed = await reuniaoRepository.RemoverAsync(id, cancellationToken);
        return removed
            ? NoContent()
            : NotFound(new { mensagem = "Registro do historico nao encontrado." });
    }

    /// <summary>
    /// Recebe o texto de uma reuniao e retorna uma analise estruturada gerada por IA.
    /// </summary>
    [HttpPost("analisar")]
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
                TopicosPrincipais = response.TopicosPrincipais,
                Acoes = response.Acoes,
                Responsaveis = response.Responsaveis,
                Decisoes = response.Decisoes,
                Pendencias = response.Pendencias,
                DataReuniao = response.DataReuniao,
                TipoReuniao = response.TipoReuniao,
                Confianca = double.IsFinite(response.NivelConfianca)
                    ? Convert.ToDecimal(response.NivelConfianca)
                    : 0m,
                GeradoPorIA = response.GeradoPorIA,
                ModoExecucao = response.ModoExecucao,
                ModeloIA = response.ModeloIA
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
