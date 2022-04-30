using System;
using System.Collections.Generic;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models
{
    public class PlayerAnalyticPerGameEntryDto
    {
        public DateTime Created { get; set; }
        public Dictionary<string, int> GameCounts { get; set; }
    }
}