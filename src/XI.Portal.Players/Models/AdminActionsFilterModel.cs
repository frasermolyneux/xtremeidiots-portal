using XI.CommonTypes;

namespace XI.Portal.Players.Models
{
    public class AdminActionsFilterModel
    {
        public enum FilterType
        {
            ActiveBans,
            UnclaimedBans
        }

        public enum OrderBy
        {
            Created,
            CreatedDesc
        }

        public GameType GameType { get; set; }
        public FilterType Filter { get; set; }
        public OrderBy Order { get; set; } = OrderBy.Created;
        public int SkipEntries { get; set; } = 0;
        public int TakeEntries { get; set; } = 0;
    }
}