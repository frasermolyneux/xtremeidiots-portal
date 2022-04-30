namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models
{
    public class BanFileMonitorDto
    {
        public Guid BanFileMonitorId { get; set; }

        public string FilePath { get; set; } = string.Empty;

        public long RemoteFileSize { get; set; }

        public DateTime LastSync { get; set; }

        public Guid ServerId { get; set; }
        public string GameType { get; set; } = string.Empty;
    }
}
