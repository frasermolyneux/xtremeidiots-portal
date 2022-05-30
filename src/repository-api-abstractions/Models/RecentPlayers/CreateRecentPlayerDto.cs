using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.RecentPlayers
{
    public class CreateRecentPlayerDto
    {
        public CreateRecentPlayerDto(string name, GameType gameType, Guid playerId)
        {
            Name = name;
            GameType = gameType;
            PlayerId = playerId;
        }

        public string Name { get; private set; }
        public string? IpAddress { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
        public string? CountryCode { get; set; }
        public GameType GameType { get; private set; }
        public Guid PlayerId { get; private set; }
        public Guid? ServerId { get; set; }
    }
}
