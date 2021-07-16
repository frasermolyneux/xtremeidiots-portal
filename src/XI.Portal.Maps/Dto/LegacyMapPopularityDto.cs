using System.Collections.Generic;
using XI.CommonTypes;

namespace XI.Portal.Maps.Dto
{
    public class LegacyMapPopularityDto
    {
        public GameType GameType { get; set; }
        public string MapName { get; set; }
        public List<LegacyMapPopularityVoteDto> MapVotes { get; set; }
    }
}