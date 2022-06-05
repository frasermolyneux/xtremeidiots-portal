using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;

public interface IGameServersEventsApi
{
    Task<ApiResponseDto> CreateGameServerEvent(CreateGameServerEventDto createGameServerEventDto);
    Task<ApiResponseDto> CreateGameServerEvents(List<CreateGameServerEventDto> createGameServerEventDtos);
}