using System.Threading.Tasks;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard
{
    public interface IRepositoryApiTokenProvider
    {
        Task<string> GetAccessToken();
    }
}