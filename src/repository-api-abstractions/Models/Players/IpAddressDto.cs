using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players
{
    public class IpAddressDto
    {
        [JsonProperty]
        public string Address { get; set; } = string.Empty;

        [JsonProperty]
        public DateTime Added { get; set; }

        [JsonProperty]
        public DateTime LastUsed { get; set; }
    }
}