using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players
{
    public class PlayersSearchResponseDto
    {
        [JsonProperty]
        public int TotalRecords { get; set; }

        [JsonProperty]
        public int FilteredRecords { get; set; }

        [JsonProperty]
        public List<PlayerDto> Entries { get; set; } = new List<PlayerDto>();
    }
}
