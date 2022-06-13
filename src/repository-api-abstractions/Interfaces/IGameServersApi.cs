using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces
{
    public interface IGameServersApi
    {
        Task<ApiResponseDto<GameServerDto>> GetGameServer(Guid gameServerId);
        Task<ApiResponseDto<GameServersCollectionDto>> GetGameServers(GameType[]? gameTypes, Guid[]? gameServerIds, GameServerFilter? filter, int skipEntries, int takeEntries, GameServerOrder? order);

        Task<ApiResponseDto> CreateGameServer(CreateGameServerDto createGameServerDto);
        Task<ApiResponseDto> CreateGameServers(List<CreateGameServerDto> createGameServerDtos);

        Task<ApiResponseDto> UpdateGameServer(EditGameServerDto editGameServerDto);

        Task<ApiResponseDto> DeleteGameServer(Guid gameServerId);
    }
}