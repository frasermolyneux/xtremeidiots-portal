using System;
using XI.CommonTypes;

namespace XI.Portal.Servers.Models
{
    public class ChatLogFilterModel
    {
        public enum OrderBy
        {
            TimestampAsc,
            TimestampDesc
        }

        public GameType GameType { get; set; }
        public Guid ServerId { get; set; }
        public Guid PlayerId { get; set; }
        public OrderBy Order { get; set; } = OrderBy.TimestampDesc;
        public string FilterString { get; set; }
        public int SkipEntries { get; set; } = 0;
        public int TakeEntries { get; set; } = 0;
    }
}