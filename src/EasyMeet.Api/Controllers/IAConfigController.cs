using EasyMeet.Api.Models;
using EasyMeet.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EasyMeet.Api.Controllers;

[ApiController]
[Route("api/ia")]
public sealed class IAConfigController(ApiKeyManagerService apiKeyManagerService, IAProviderFactory providerFactory) : ControllerBase
{
    [HttpGet("provedores")]
    [ProducesResponseType(typeof(IReadOnlyList<ProvedorIAResponse>), StatusCodes.Status200OK)]
    public IActionResult ListarProvedores()
    {
        var list = providerFactory.ListarProvedores()
            .Select(x => new ProvedorIAResponse { ProvedorIA = x, Nome = x.ToString() })
            .ToArray();
        return Ok(list);
    }

    [HttpGet("credenciais/status")]
    [ProducesResponseType(typeof(IReadOnlyList<ApiKeyStatusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterStatus(CancellationToken cancellationToken)
    {
        var status = await apiKeyManagerService.ObterStatusAsync(cancellationToken);
        return Ok(status);
    }

    [HttpPost("credenciais")]
    public async Task<IActionResult> SalvarCredencial([FromBody] SalvarApiKeyRequest request, CancellationToken cancellationToken)
    {
        await apiKeyManagerService.SalvarOuAtualizarAsync(request.ProvedorIA, request.ApiKey, cancellationToken);
        return Ok(new { mensagem = "Chave salva com sucesso." });
    }

    [HttpDelete("credenciais/{provedor}")]
    public async Task<IActionResult> RemoverCredencial([FromRoute] ProvedorIA provedor, CancellationToken cancellationToken)
    {
        await apiKeyManagerService.RemoverAsync(provedor, cancellationToken);
        return Ok(new { mensagem = "Chave removida com sucesso." });
    }

    [HttpPost("credenciais/testar")]
    public async Task<IActionResult> TestarCredencial([FromBody] TestarApiKeyRequest request, CancellationToken cancellationToken)
    {
        var resultado = await apiKeyManagerService.TestarConexaoDetalhadoAsync(request.ProvedorIA, request.ApiKey, cancellationToken);
        return Ok(resultado);
    }
}
