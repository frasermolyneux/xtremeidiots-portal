using System;
using System.Collections.Generic;
using System.Text;
using XI.CommonTypes;

namespace XI.Portal.Maps.Dto
{
    public class MapPopularityDto
    {
        public GameType GameType { get; set; }
        public string MapName { get; set; }
        public List<MapPopularityVoteDto> MapVotes { get; set; }
    }
}
