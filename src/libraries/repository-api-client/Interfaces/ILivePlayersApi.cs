using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.RepositoryApiClient.Interfaces
{
    public interface ILivePlayersApi
    {
        Task<List<LivePlayerDto>?> CreateGameServerLivePlayers(Guid serverId, List<LivePlayerDto> livePlayerDtos);
        Task<LivePlayersResponseDto?> GetLivePlayers(GameType? gameType, Guid? serverId, LivePlayerFilter? filter);
    }
}
