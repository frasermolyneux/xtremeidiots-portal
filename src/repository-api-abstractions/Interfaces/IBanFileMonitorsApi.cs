using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.BanFileMonitors;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces
{
    public interface IBanFileMonitorsApi
    {
        Task<ApiResponseDto<BanFileMonitorDto>> GetBanFileMonitor(Guid banFileMonitorId);
        Task<ApiResponseDto<BanFileMonitorCollectionDto>> GetBanFileMonitors(GameType[]? gameTypes, Guid[]? banFileMonitorIds, Guid? serverId, int skipEntries, int takeEntries, BanFileMonitorOrder? order);

        Task<ApiResponseDto> CreateBanFileMonitor(CreateBanFileMonitorDto createBanFileMonitorDto);

        Task<ApiResponseDto> UpdateBanFileMonitor(EditBanFileMonitorDto editBanFileMonitorDto);

        Task<ApiResponseDto> DeleteBanFileMonitor(Guid banFileMonitorId);
    }
}
