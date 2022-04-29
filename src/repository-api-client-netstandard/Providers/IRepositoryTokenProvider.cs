using System.Threading.Tasks;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.Providers
{
    public interface IRepositoryTokenProvider
    {
        Task<string> GetRepositoryAccessToken();
    }
}