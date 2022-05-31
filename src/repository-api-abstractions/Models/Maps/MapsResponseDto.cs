namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models
{
    public class MapsResponseDto
    {
        public int TotalRecords { get; set; }
        public int FilteredRecords { get; set; }
        public List<MapDto> Entries { get; set; } = new List<MapDto>();
    }
}
