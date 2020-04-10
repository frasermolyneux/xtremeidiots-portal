using XI.CommonTypes;

namespace XI.Portal.Players.Models
{
    public class PlayersFilterModel
    {
        public enum FilterType
        {
            None,
            UsernameAndGuid,
            IpAddress
        }

        public enum OrderBy
        {
            UsernameAsc,
            UsernameDesc,
            FirstSeenAsc,
            FirstSeenDesc,
            LastSeenAsc,
            LastSeenDesc
        }

        public GameType GameType { get; set; }
        public FilterType Filter { get; set; } = FilterType.None;
        public OrderBy Order { get; set; } = OrderBy.LastSeenDesc;
        public string FilterString { get; set; }
        public int SkipPlayers { get; set; } = 0;
        public int TakePlayers { get; set; } = 0;
    }
}