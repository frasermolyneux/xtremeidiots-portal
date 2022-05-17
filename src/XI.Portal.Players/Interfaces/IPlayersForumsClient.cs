using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XI.Portal.Players.Interfaces
{
    public interface IPlayersForumsClient
    {
        Task<int> CreateTopicForAdminAction(AdminActionDto model);
        Task UpdateTopicForAdminAction(AdminActionDto model);
    }
}