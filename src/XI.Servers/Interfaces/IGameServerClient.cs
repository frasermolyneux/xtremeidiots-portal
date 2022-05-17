using XI.Servers.Dto;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XI.Servers.Interfaces
{
    public interface IGameServerClient
    {
        void Configure(GameType gameType, Guid serverId, string hostname, int queryPort, string rconPassword);
        Task<GameServerStatusDto> GetServerStatus();
    }
}