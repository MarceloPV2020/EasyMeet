using System.ComponentModel.DataAnnotations;

namespace EasyMeet.Api.Models;

/// <summary>
/// Requisição contendo texto de reunião para análise.
/// </summary>
public sealed class ResumoReuniaoRequest
{
    /// <summary>
    /// Texto integral da reunião a ser analisado pela IA.
    /// </summary>
    [Required(ErrorMessage = "O campo texto é obrigatório.")]
    public string Texto { get; init; } = string.Empty;
}
