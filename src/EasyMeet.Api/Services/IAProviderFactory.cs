using EasyMeet.Api.Models;

namespace EasyMeet.Api.Services;

public interface IAProviderFactory
{
    IGenerativeAIClient GetClient(ProvedorIA provedorIA);
    IReadOnlyList<ProvedorIA> ListarProvedores();
}
