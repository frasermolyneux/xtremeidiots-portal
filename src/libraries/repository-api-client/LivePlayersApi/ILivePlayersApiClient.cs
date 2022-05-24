using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.LivePlayersApi
{
    public interface ILivePlayersApiClient
    {
        Task<List<LivePlayerDto>?> CreateGameServerLivePlayers(Guid serverId, List<LivePlayerDto> livePlayerDtos);
        Task<LivePlayersResponseDto?> GetLivePlayers(GameType? gameType, Guid? serverId, LivePlayerFilter? filter);
    }
}
