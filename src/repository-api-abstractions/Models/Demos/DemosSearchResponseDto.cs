using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Demos
{
    public class DemosSearchResponseDto
    {
        [JsonProperty]
        public int TotalRecords { get; set; }

        [JsonProperty]
        public int FilteredRecords { get; set; }

        [JsonProperty]
        public List<DemoDto> Entries { get; set; } = new List<DemoDto>();
    }
}
