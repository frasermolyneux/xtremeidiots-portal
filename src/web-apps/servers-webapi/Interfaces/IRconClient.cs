using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.ServersWebApi.Interfaces
{
    public interface IRconClient
    {
        void Configure(GameType gameType, Guid gameServerId, string hostname, int queryPort, string rconPassword);
        List<IRconPlayer> GetPlayers();
        Task Say(string message);
        Task<string> Restart();
        Task<string> RestartMap();
        Task<string> FastRestartMap();
        Task<string> NextMap();
    }
}