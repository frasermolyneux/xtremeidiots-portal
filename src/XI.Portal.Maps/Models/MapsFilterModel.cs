using System.Collections.Generic;
using XI.CommonTypes;

namespace XI.Portal.Maps.Models
{
    public class MapsFilterModel
    {
        public enum OrderBy
        {
            MapNameAsc,
            MapNameDesc,
            LikeDislikeAsc,
            LikeDislikeDesc,
            GameTypeAsc,
            GameTypeDesc
        }

        public GameType GameType { get; set; }
        public OrderBy Order { get; set; } = OrderBy.MapNameAsc;
        public List<string> MapNames { get; set; }
        public string FilterString { get; set; }
        public int SkipEntries { get; set; } = 0;
        public int TakeEntries { get; set; } = 0;
    }
}