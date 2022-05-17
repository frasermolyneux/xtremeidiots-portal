using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XI.Portal.Servers.Dto
{
    public class PortalGameServerStatusDto
    {
        public Guid ServerId { get; set; }
        public GameType GameType { get; set; }
        public string Hostname { get; set; }
        public int QueryPort { get; set; }
        public int MaxPlayers { get; set; }
        public string ServerName { get; set; }
        public string Map { get; set; }
        public string Mod { get; set; }
        public int PlayerCount { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public IList<PortalGameServerPlayerDto> Players { get; set; }
    }
}