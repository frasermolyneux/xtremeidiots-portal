using System;
using System.Collections.Generic;
using XI.CommonTypes;

namespace XI.Portal.Servers.Models
{
    public class BanFileMonitorFilterModel
    {
        public List<GameType> GameTypes { get; set; }
        public List<Guid> BanFileMonitorIds { get; set; }

        public int SkipEntries { get; set; } = 0;
        public int TakeEntries { get; set; } = 0;
    }
}