using Newtonsoft.Json;
using System.Text.Json.Serialization;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.BanFileMonitors
{
    public class BanFileMonitorDto
    {
        [JsonProperty]
        public Guid BanFileMonitorId { get; set; }

        [JsonProperty]
        public string FilePath { get; set; } = string.Empty;

        [JsonProperty]
        public long RemoteFileSize { get; set; }

        [JsonProperty]
        public DateTime LastSync { get; set; }

        [JsonProperty]
        public Guid ServerId { get; set; }

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; set; }
    }
}
