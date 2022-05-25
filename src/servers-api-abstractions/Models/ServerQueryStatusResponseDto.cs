namespace XtremeIdiots.Portal.ServersApi.Abstractions.Models
{
    public class ServerQueryStatusResponseDto
    {
        public string? ServerName { get; set; }
        public string? Map { get; set; }
        public string? Mod { get; set; }
        public int MaxPlayers { get; set; }
        public int PlayerCount { get; set; }

        public IDictionary<string, string> ServerParams { get; set; } = new Dictionary<string, string>();
        public IList<ServerQueryPlayerDto> Players { get; set; } = new List<ServerQueryPlayerDto>();
    }
}
