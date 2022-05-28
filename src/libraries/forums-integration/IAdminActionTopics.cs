using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.ForumsIntegration
{
    public interface IAdminActionTopics
    {
        Task<int> CreateTopicForAdminAction(AdminActionDto model);
        Task UpdateTopicForAdminAction(AdminActionDto model);
    }
}
