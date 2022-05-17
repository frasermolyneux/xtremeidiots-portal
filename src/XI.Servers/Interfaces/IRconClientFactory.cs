﻿using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XI.Servers.Interfaces
{
    public interface IRconClientFactory
    {
        IRconClient CreateInstance(GameType gameType, Guid serverId, string hostname, int queryPort, string rconPassword);
    }
}