using System;
using System.Collections.Generic;
using XI.CommonTypes;

namespace XI.Portal.Servers.Models
{
    public class GameServerFilterModel
    {
        public enum FilterBy
        {
            None,
            ShowOnPortalServerList
        }

        public enum OrderBy
        {
            None,
            BannerServerListPosition,
            GameType
        }

        public List<GameType> GameTypes { get; set; }
        public List<Guid> ServerIds { get; set; }
        public OrderBy Order { get; set; } = OrderBy.None;
        public FilterBy Filter { get; set; } = FilterBy.None;
        public int SkipEntries { get; set; } = 0;
        public int TakeEntries { get; set; } = 0;
    }
}