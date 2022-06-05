using Newtonsoft.Json;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players
{
    public class PlayerAnalyticPerGameEntryDto
    {
        [JsonProperty]
        public DateTime Created { get; set; }

        [JsonProperty]
        public Dictionary<GameType, int> GameCounts { get; set; } = new Dictionary<GameType, int>();
    }
}