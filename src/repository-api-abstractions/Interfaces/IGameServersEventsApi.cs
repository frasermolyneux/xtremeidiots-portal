using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;

public interface IGameServersEventsApi
{
    Task CreateGameServerEvent(GameServerEventDto gameServerEvent);
}