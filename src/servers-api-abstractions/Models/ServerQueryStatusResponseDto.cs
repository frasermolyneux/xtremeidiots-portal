namespace XtremeIdiots.Portal.ServersApi.Abstractions.Models
{
    public class ServerQueryStatusResponseDto
    {
        public string ServerName { get; set; } = string.Empty;
        public string Map { get; set; } = string.Empty;
        public string Mod { get; set; } = string.Empty;
        public int MaxPlayers { get; set; }
        public int PlayerCount { get; set; }

        public IDictionary<string, string> ServerParams { get; set; } = new Dictionary<string, string>();
        public IList<ServerQueryPlayerDto> Players { get; set; } = new List<ServerQueryPlayerDto>();
    }
}
