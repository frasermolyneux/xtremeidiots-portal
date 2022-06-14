using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.BanFileMonitors
{
    public class EditBanFileMonitorDto
    {
        [JsonConstructor]
        public EditBanFileMonitorDto(Guid banFileMonitorId)
        {
            BanFileMonitorId = banFileMonitorId;
        }

        public EditBanFileMonitorDto(Guid banFileMonitorId, string filePath)
        {
            BanFileMonitorId = banFileMonitorId;
            FilePath = filePath;
        }

        public EditBanFileMonitorDto(Guid banFileMonitorId, long remoteFileSize, DateTime lastSync)
        {
            BanFileMonitorId = banFileMonitorId;
            RemoteFileSize = remoteFileSize;
            LastSync = lastSync;
        }

        [JsonProperty]
        public Guid BanFileMonitorId { get; private set; }

        [JsonProperty]
        public string? FilePath { get; set; }

        [JsonProperty]
        public long? RemoteFileSize { get; set; }

        [JsonProperty]
        public DateTime? LastSync { get; set; }
    }
}
