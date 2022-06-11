using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces
{
    public interface IPlayersApi
    {
        Task<ApiResponseDto<PlayerDto>> GetPlayer(Guid playerId);
        Task<ApiResponseDto<PlayerDto>> GetPlayerByGameType(GameType gameType, string guid);
        Task<ApiResponseDto<PlayersCollectionDto>> GetPlayers(GameType? gameType, PlayersFilter? filter, string? filterString, int skipEntries, int takeEntries, PlayersOrder? order);

        Task<ApiResponseDto> CreatePlayer(CreatePlayerDto createPlayerDto);
        Task<ApiResponseDto> CreatePlayers(List<CreatePlayerDto> createPlayerDtos);

        Task<ApiResponseDto> UpdatePlayer(EditPlayerDto editPlayerDto);
    }
}