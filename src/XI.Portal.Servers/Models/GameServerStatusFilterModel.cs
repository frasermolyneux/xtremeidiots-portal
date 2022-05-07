using System;
using System.Collections.Generic;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Portal.Servers.Models
{
    public class GameServerStatusFilterModel
    {
        public List<GameType> GameTypes { get; set; }
        public List<Guid> ServerIds { get; set; }
    }
}