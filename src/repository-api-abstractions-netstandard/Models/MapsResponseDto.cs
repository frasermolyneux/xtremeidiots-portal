using System.Collections.Generic;
using XI.Portal.Repository.Dtos;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models
{
    public class MapsResponseDto
    {
        public int TotalRecords { get; set; }
        public int FilteredRecords { get; set; }
        public List<MapDto> Entries { get; set; } = new List<MapDto>();
    }
}
