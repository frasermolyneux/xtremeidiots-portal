using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players
{
    public class LivePlayersResponseDto
    {
        [JsonProperty]
        public int TotalRecords { get; set; }

        [JsonProperty]
        public int FilteredRecords { get; set; }

        [JsonProperty]
        public List<LivePlayerDto> Entries { get; set; } = new List<LivePlayerDto>();
    }
}
