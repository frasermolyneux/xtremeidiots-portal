using System.Collections.Generic;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models
{
    public class PlayersSearchResponseDto
    {
        public int TotalRecords { get; set; }
        public int FilteredRecords { get; set; }
        public List<PlayerDto> Entries { get; set; } = new List<PlayerDto>();
    }
}
