using System.Threading.Tasks;

namespace XtremeIdiots.Portal.FuncHelpers.Providers
{
    public interface IRepositoryTokenProvider
    {
        Task<string> GetRepositoryAccessToken();
    }
}