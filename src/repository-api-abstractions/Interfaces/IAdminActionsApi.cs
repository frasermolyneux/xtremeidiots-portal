using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces
{
    public interface IAdminActionsApi
    {
        Task<ApiResponseDto<AdminActionDto>> GetAdminAction(Guid adminActionId);
        Task<ApiResponseDto<AdminActionCollectionDto>> GetAdminActions(GameType? gameType, Guid? playerId, string? adminId, AdminActionFilter? filter, int skipEntries, int takeEntries, AdminActionOrder? order);

        Task<ApiResponseDto> CreateAdminAction(CreateAdminActionDto createAdminActionDto);

        Task<ApiResponseDto> UpdateAdminAction(EditAdminActionDto editAdminActionDto);

        Task<ApiResponseDto> DeleteAdminAction(Guid adminActionId);
    }
}