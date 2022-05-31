namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models
{
    public class GameServerStatDto
    {
        public Guid Id { get; set; }
        public Guid GameServerId { get; set; }
        public int PlayerCount { get; set; }
        public string MapName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
