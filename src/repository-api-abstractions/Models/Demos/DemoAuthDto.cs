using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Demos
{
    public class DemoAuthDto
    {
        [JsonProperty]
        public string UserId { get; set; } = string.Empty;

        [JsonProperty]
        public string AuthKey { get; set; } = string.Empty;

        [JsonProperty]
        public DateTime Created { get; set; }

        [JsonProperty]
        public DateTime LastActivity { get; set; }
    }
}
