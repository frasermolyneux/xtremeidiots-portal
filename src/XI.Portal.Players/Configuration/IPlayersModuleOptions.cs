using System;

namespace XI.Portal.Players.Configuration
{
    public interface IPlayersModuleOptions
    {
        Action<IPlayersRepositoryOptions> PlayersRepositoryOptions { get; set; }

        void Validate();
    }
}