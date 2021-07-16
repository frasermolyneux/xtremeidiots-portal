using System;
using System.Collections.Generic;
using System.ComponentModel;
using XI.CommonTypes;

namespace XI.Portal.Maps.Dto
{
    public class MapDto
    {
        public Guid MapId { get; set; }
        [DisplayName("Game")] public GameType GameType { get; set; }
        [DisplayName("Map Name")] public string MapName { get; set; }
        [DisplayName("Map File")] public IList<MapFileDto> MapFiles { get; set; }
        [DisplayName("Map Votes")] public IList<LegacyMapVoteDto> MapVotes { get; set; }
        public double LikePercentage { get; set; }
        public double DislikePercentage { get; set; }
        public double TotalLikes { get; set; }
        public double TotalDislikes { get; set; }
        public int TotalVotes { get; set; }
    }
}