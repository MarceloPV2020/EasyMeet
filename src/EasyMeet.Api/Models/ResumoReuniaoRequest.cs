using System.ComponentModel.DataAnnotations;

namespace EasyMeet.Api.Models;

public sealed class ResumoReuniaoRequest
{
    [Required(ErrorMessage = "O campo transcricao e obrigatorio.")]
    public string Transcricao { get; init; } = string.Empty;

    [Required]
    public ProvedorIA ProvedorIA { get; init; } = ProvedorIA.Gemini;

    public ConfiguracaoAnaliseIA? ConfiguracaoIA { get; init; }
}
