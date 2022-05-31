using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.BanFileMonitors;

namespace XtremeIdiots.Portal.RepositoryApiClient.BanFileMonitorsApi
{
    public interface IBanFileMonitorsApiClient
    {
        Task<BanFileMonitorDto?> GetBanFileMonitor(Guid banFileMonitorId);
        Task<List<BanFileMonitorDto>?> GetBanFileMonitors(GameType[]? gameTypes, Guid[]? banFileMonitorIds, Guid? serverId, int skipEntries, int takeEntries, string? order);
        Task<BanFileMonitorDto?> UpdateBanFileMonitor(BanFileMonitorDto banFileMonitor);
        Task DeleteBanFileMonitor(Guid banFileMonitorId);
    }
}
