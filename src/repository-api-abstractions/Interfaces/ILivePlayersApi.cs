using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces
{
    public interface ILivePlayersApi
    {
        Task<ApiResponseDto<LivePlayersCollectionDto>> GetLivePlayers(GameType? gameType, Guid? gameServerId, LivePlayerFilter? filter, int skipEntries, int takeEntries, LivePlayersOrder? order);

        Task<ApiResponseDto> SetLivePlayersForGameServer(Guid gameServerId, List<CreateLivePlayerDto> createLivePlayerDtos);
    }
}
