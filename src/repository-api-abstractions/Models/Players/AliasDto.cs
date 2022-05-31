using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players
{
    public class AliasDto
    {
        [JsonProperty]
        public string Name { get; set; } = string.Empty;

        [JsonProperty]
        public DateTime Added { get; set; }

        [JsonProperty]
        public DateTime LastUsed { get; set; }
    }
}