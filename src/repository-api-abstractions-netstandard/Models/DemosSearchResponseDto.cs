using System.Collections.Generic;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models
{
    public class DemosSearchResponseDto
    {
        public int TotalRecords { get; set; }
        public int FilteredRecords { get; set; }
        public List<DemoDto> Entries { get; set; } = new List<DemoDto>();
    }
}
