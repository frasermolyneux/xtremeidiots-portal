using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces
{
    public interface IPlayersApi
    {
        Task<ApiResponseDto<PlayerDto>> GetPlayer(Guid playerId);
        Task<ApiResponseDto<PlayerDto>> GetPlayerByGameType(GameType gameType, string guid);
        Task<ApiResponseDto> CreatePlayer(CreatePlayerDto createPlayerDto);
        Task<ApiResponseDto> CreatePlayers(List<CreatePlayerDto> createPlayerDtos);
        Task<ApiResponseDto> UpdatePlayer(EditPlayerDto editPlayerDto);
        Task<PlayersSearchResponseDto?> SearchPlayers(string gameType, string filter, string filterString, int takeEntries, int skipEntries, string? order);
        Task<List<AdminActionDto>?> GetAdminActionsForPlayer(Guid playerId);
        Task<AdminActionDto?> CreateAdminActionForPlayer(AdminActionDto adminAction);
        Task<AdminActionDto?> UpdateAdminActionForPlayer(AdminActionDto adminAction);
    }
}