using EasyMeet.Api.Models;

namespace EasyMeet.Api.Services;

public interface IApiKeyStore
{
    Task SaveApiKeyAsync(ProvedorIA provedor, string apiKey, CancellationToken cancellationToken);
    Task<string?> GetApiKeyAsync(ProvedorIA provedor, CancellationToken cancellationToken);
    Task DeleteApiKeyAsync(ProvedorIA provedor, CancellationToken cancellationToken);
    Task<bool> HasApiKeyAsync(ProvedorIA provedor, CancellationToken cancellationToken);
}
