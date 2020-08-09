using System;
using System.Collections.Generic;

namespace XI.Portal.Players.Dto
{
    public class PlayerAnalyticPerGameEntryDto
    {
        public DateTime Created { get; set; }
        public Dictionary<string, int> GameCounts { get; set; }
    }
}