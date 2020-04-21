using System;
using XI.Portal.Players.Configuration;

namespace XI.Portal.Players.Interfaces
{
    public interface IPlayersModuleOptions
    {
        Action<IPlayersRepositoryOptions> PlayersRepositoryOptions { get; set; }
        Action<IAdminActionsRepositoryOptions> AdminActionsRepositoryOptions { get; set; }

        void Validate();
    }
}