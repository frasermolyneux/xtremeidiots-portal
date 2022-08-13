using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players
{
    public class IpAddressDto
    {
        [JsonProperty]
        public string Address { get; internal set; } = string.Empty;

        [JsonProperty]
        public DateTime Added { get; internal set; }

        [JsonProperty]
        public DateTime LastUsed { get; internal set; }

        [JsonProperty]
        public int ConfidenceScore { get; internal set; }
    }
}