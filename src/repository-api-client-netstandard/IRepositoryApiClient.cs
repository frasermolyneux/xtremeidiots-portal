﻿using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.BanFileMonitorsApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.ChatMessagesApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.GameServersApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.PlayersApi;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard
{
    public interface IRepositoryApiClient
    {
        IBanFileMonitorsApiClient BanFileMonitors { get; }
        IChatMessagesApiClient ChatMessages { get; }
        IGameServersApiClient GameServers { get; }
        IPlayersApiClient Players { get; }
    }
}