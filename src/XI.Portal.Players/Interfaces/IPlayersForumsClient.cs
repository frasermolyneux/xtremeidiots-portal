using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XI.Portal.Players.Interfaces
{
    public interface IPlayersForumsClient
    {
        Task<int> CreateTopicForAdminAction(AdminActionDto model);
        Task UpdateTopicForAdminAction(AdminActionDto model);
    }
}