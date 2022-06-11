using Newtonsoft.Json;

using System.Text.Json.Serialization;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.BanFileMonitors
{
    public class BanFileMonitorDto
    {
        [JsonProperty]
        public Guid BanFileMonitorId { get; internal set; }

        [JsonProperty]
        public string FilePath { get; internal set; } = string.Empty;

        [JsonProperty]
        public long RemoteFileSize { get; internal set; }

        [JsonProperty]
        public DateTime LastSync { get; internal set; }

        [JsonProperty]
        public Guid ServerId { get; internal set; }

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; internal set; }

        [JsonProperty]
        public GameServerDto? GameServerDto { get; internal set; }
    }
}
