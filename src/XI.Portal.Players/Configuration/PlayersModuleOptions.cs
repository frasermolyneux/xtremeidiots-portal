using XI.Portal.Players.Interfaces;

namespace XI.Portal.Players.Configuration
{
    internal class PlayersModuleOptions : IPlayersModuleOptions
    {
        public Action<IPlayerLocationsRepositoryOptions> PlayerLocationsRepositoryOptions { get; set; }
        public Action<IPlayersCacheRepositoryOptions> PlayersCacheRepositoryOptions { get; set; }
        public Action<IBanFilesRepositoryOptions> BanFilesRepositoryOptions { get; set; }
    }
}