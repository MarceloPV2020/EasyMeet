using EasyMeet.Api.Models;

namespace EasyMeet.Api.Services;

public interface IReuniaoRepository
{
    Task InicializarAsync(CancellationToken cancellationToken = default);
    Task<int> AdicionarAsync(Reuniao reuniao, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Reuniao>> ListarAsync(CancellationToken cancellationToken = default);
    Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);
}
