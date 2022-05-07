using System;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Portal.Servers.Dto
{
    public class GameServerStatusStatsDto
    {
        public Guid ServerId { get; set; }
        public GameType GameType { get; set; }

        public int PlayerCount { get; set; }
        public string MapName { get; set; }

        public DateTimeOffset Timestamp { get; set; }
    }
}