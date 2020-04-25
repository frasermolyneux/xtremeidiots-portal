using System;

namespace XI.Portal.Players.Interfaces
{
    public interface IPlayersModuleOptions
    {
        Action<IPlayersRepositoryOptions> PlayersRepositoryOptions { get; set; }
        Action<IPlayerLocationsRepositoryOptions> PlayerLocationsRepositoryOptions { get; set; }

        void Validate();
    }
}