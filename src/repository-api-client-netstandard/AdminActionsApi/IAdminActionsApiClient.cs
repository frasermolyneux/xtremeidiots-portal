using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.AdminActionsApi
{
    public interface IAdminActionsApiClient
    {
        Task<AdminActionDto> GetAdminAction(string accessToken, Guid adminActionId);
        Task<List<AdminActionDto>> GetAdminActions(string accessToken, string gameType, Guid? playerId, string adminId, string filterType, int skipEntries, int takeEntries, string order);
        Task DeleteAdminAction(string accessToken, Guid adminActionId);
    }
}