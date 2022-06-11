using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Demos
{
    public class DemoAuthDto
    {
        [JsonProperty]
        public string UserId { get; internal set; } = string.Empty;

        [JsonProperty]
        public string AuthKey { get; internal set; } = string.Empty;

        [JsonProperty]
        public DateTime Created { get; internal set; }

        [JsonProperty]
        public DateTime LastActivity { get; internal set; }
    }
}
