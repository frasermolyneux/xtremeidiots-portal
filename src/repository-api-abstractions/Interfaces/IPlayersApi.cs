using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces
{
    public interface IPlayersApi
    {
        Task<ApiResponseDto<PlayerDto>> GetPlayer(Guid playerId);
        Task<List<RelatedPlayerDto>?> GetRelatedPlayers(Guid playerId, string ipAddress);
        Task<PlayerDto?> GetPlayerByGameType(GameType gameType, string guid);
        Task CreatePlayer(CreatePlayerDto createPlayerDto);
        Task UpdatePlayer(PlayerDto player);
        Task<PlayersSearchResponseDto?> SearchPlayers(string gameType, string filter, string filterString, int takeEntries, int skipEntries, string? order);
        Task<List<AdminActionDto>?> GetAdminActionsForPlayer(Guid playerId);
        Task<AdminActionDto?> CreateAdminActionForPlayer(AdminActionDto adminAction);
        Task<AdminActionDto?> UpdateAdminActionForPlayer(AdminActionDto adminAction);
    }
}