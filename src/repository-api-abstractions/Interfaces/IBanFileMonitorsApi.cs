using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.BanFileMonitors;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces
{
    public interface IBanFileMonitorsApi
    {
        Task<BanFileMonitorDto?> GetBanFileMonitor(Guid banFileMonitorId);
        Task<List<BanFileMonitorDto>?> GetBanFileMonitors(GameType[]? gameTypes, Guid[]? banFileMonitorIds, Guid? serverId, int skipEntries, int takeEntries, string? order);
        Task<BanFileMonitorDto?> UpdateBanFileMonitor(BanFileMonitorDto banFileMonitor);
        Task DeleteBanFileMonitor(Guid banFileMonitorId);
        Task<BanFileMonitorDto?> CreateBanFileMonitorForGameServer(Guid serverId, BanFileMonitorDto banFileMonitor);
    }
}
