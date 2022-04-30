namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models
{
    public class GameServerEventDto
    {
        public Guid Id { get; set; }
        public string GameServerId { get; set; }
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; }
        public string EventData { get; set; }
    }
}