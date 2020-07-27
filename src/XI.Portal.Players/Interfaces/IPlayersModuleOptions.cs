using System;

namespace XI.Portal.Players.Interfaces
{
    public interface IPlayersModuleOptions
    {
        Action<IPlayerLocationsRepositoryOptions> PlayerLocationsRepositoryOptions { get; set; }
        Action<IPlayersCacheRepositoryOptions> PlayersCacheRepositoryOptions { get; set; }
        Action<IBanFilesRepositoryOptions> BanFilesRepositoryOptions { get; set; }
    }
}