﻿using System;
using System.Threading.Tasks;
using XI.CommonTypes;
using XI.Servers.Dto;

namespace XI.Servers.Helpers
{
    public interface IGameServerStatusHelper
    {
        void Configure(GameType gameType, Guid serverId, string hostname, int queryPort, string rconPassword);
        Task<GameServerStatusDto> GetServerStatus();
    }
}