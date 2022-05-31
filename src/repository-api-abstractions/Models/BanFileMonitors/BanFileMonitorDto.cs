using System.Text.Json.Serialization;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.BanFileMonitors
{
    public class BanFileMonitorDto
    {
        public Guid BanFileMonitorId { get; set; }

        public string FilePath { get; set; } = string.Empty;

        public long RemoteFileSize { get; set; }

        public DateTime LastSync { get; set; }

        public Guid ServerId { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; set; }
    }
}
