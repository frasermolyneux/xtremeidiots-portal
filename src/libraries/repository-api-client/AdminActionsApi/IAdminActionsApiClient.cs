using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.AdminActionsApi
{
    public interface IAdminActionsApiClient
    {
        Task<AdminActionDto?> GetAdminAction(Guid adminActionId);
        Task<List<AdminActionDto>?> GetAdminActions(string gameType, Guid? playerId, string adminId, string filterType, int skipEntries, int takeEntries, string order);
        Task DeleteAdminAction(Guid adminActionId);
    }
}