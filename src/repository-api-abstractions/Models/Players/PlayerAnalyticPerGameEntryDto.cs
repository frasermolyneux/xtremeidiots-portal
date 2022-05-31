using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players
{
    public class PlayerAnalyticPerGameEntryDto
    {
        [JsonProperty]
        public DateTime Created { get; set; }

        [JsonProperty]
        public Dictionary<string, int> GameCounts { get; set; } = new Dictionary<string, int>();
    }
}