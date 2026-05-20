using EasyMeet.Api.Models;

namespace EasyMeet.Api.Services;

public sealed class AIProviderFactory(IEnumerable<IGenerativeAIClient> providers) : IAProviderFactory
{
    private readonly Dictionary<ProvedorIA, IGenerativeAIClient> _providers = providers.ToDictionary(x => x.Provedor);

    public IGenerativeAIClient GetClient(ProvedorIA provedorIA)
    {
        if (_providers.TryGetValue(provedorIA, out var provider))
        {
            return provider;
        }

        throw new InvalidOperationException($"Provedor de IA nao suportado: {provedorIA}");
    }

    public IReadOnlyList<ProvedorIA> ListarProvedores()
    {
        return _providers.Keys.OrderBy(x => x).ToArray();
    }
}
