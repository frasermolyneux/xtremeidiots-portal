using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players
{
    public class PlayerAnalyticEntryDto
    {
        [JsonProperty]
        public DateTime Created { get; set; }

        [JsonProperty]
        public int Count { get; set; }
    }
}