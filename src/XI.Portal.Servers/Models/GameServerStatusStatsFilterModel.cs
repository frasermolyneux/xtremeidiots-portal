using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XI.Portal.Servers.Models
{
    public class GameServerStatusStatsFilterModel
    {
        public enum OrderBy
        {
            None,
            TimestampAsc,
            TimestampDesc
        }

        public Guid ServerId { get; set; }
        public GameType GameType { get; set; }
        public DateTime? Cutoff { get; set; }
        public OrderBy Order { get; set; } = OrderBy.None;
    }
}