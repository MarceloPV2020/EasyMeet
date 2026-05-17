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

    /// <summary>
    /// Idioma principal da reunião.
    /// </summary>
    [Required(ErrorMessage = "O campo idioma é obrigatório.")]
    public string Idioma { get; init; } = "pt-BR";
}
