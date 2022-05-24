using XI.Portal.Players.Interfaces;

namespace XI.Portal.Players.Configuration
{
    internal class PlayersModuleOptions : IPlayersModuleOptions
    {
        public Action<IBanFilesRepositoryOptions> BanFilesRepositoryOptions { get; set; }
    }
}