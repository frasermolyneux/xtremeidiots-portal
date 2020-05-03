using System;

namespace XI.Portal.Players.Interfaces
{
    public interface IPlayersModuleOptions
    {
        Action<IPlayerLocationsRepositoryOptions> PlayerLocationsRepositoryOptions { get; set; }
    }
}