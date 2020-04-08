using System.Collections.Generic;

namespace XI.Portal.Maps.Models
{
    public class MapsListEntryViewModel
    {
        public string GameType { get; set; }
        public string MapName { get; set; }
        public int TotalVotes { get; set; }
        public double TotalLikes { get; set; }
        public double TotalDislikes { get; set; }
        public double LikePercentage { get; set; }
        public double DislikePercentage { get; set; }
        public Dictionary<string, string> MapFiles { get; set; }
    }
}