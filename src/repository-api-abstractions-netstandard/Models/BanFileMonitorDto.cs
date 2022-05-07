using System;
using System.Text.Json.Serialization;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models
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
