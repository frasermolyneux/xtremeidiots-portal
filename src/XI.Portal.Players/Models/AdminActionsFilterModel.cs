using System;
using XI.CommonTypes;

namespace XI.Portal.Players.Models
{
    public class AdminActionsFilterModel
    {
        public enum FilterType
        {
            None,
            ActiveBans,
            UnclaimedBans
        }

        public enum OrderBy
        {
            None,
            CreatedAsc,
            CreatedDesc
        }

        public GameType GameType { get; set; }
        public Guid PlayerId { get; set; }
        public FilterType Filter { get; set; } = FilterType.None;
        public OrderBy Order { get; set; } = OrderBy.None;
        public int SkipEntries { get; set; } = 0;
        public int TakeEntries { get; set; } = 0;
    }
}