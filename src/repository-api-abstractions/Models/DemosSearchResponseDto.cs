namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models
{
    public class DemosSearchResponseDto
    {
        public int TotalRecords { get; set; }
        public int FilteredRecords { get; set; }
        public List<DemoDto> Entries { get; set; } = new List<DemoDto>();
    }
}
