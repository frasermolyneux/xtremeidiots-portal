using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.RecentPlayers
{
    public class RecentPlayerDto
    {
        public Guid Id { get; internal set; }
        public string? Name { get; internal set; }
        public string? IpAddress { get; internal set; }
        public double Lat { get; internal set; }
        public double Long { get; internal set; }
        public string? CountryCode { get; internal set; }
        public GameType GameType { get; internal set; }
        public Guid? PlayerId { get; internal set; }
        public Guid? ServerId { get; internal set; }
        public DateTime Timestamp { get; internal set; }
    }
}
