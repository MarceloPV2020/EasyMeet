using System.ComponentModel.DataAnnotations;

namespace EasyMeet.Api.Models;

public sealed class TestarApiKeyRequest
{
    [Required]
    public ProvedorIA ProvedorIA { get; init; }

    [Required]
    [MinLength(8)]
    public string ApiKey { get; init; } = string.Empty;
}
