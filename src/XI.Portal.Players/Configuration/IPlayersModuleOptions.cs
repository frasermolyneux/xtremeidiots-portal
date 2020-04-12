using System;

namespace XI.Portal.Players.Configuration
{
    public interface IPlayersModuleOptions
    {
        Action<IPlayersRepositoryOptions> PlayersRepositoryOptions { get; set; }
        Action<IAdminActionsRepositoryOptions> AdminActionsRepositoryOptions { get; set; }

        void Validate();
    }
}