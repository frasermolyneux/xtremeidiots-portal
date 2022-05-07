using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.GameServersEventsApi
{
    public interface IGameServersEventsApiClient
    {
        Task CreateGameServerEvent(string accessToken, GameServerEventDto gameServerEvent);
    }
}