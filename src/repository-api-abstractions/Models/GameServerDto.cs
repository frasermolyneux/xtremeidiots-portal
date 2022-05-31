using System.Text.Json.Serialization;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models
{
    public class GameServerDto
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? HtmlBanner { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; set; }
        public string? Hostname { get; set; }
        public int QueryPort { get; set; }
        public string? FtpHostname { get; set; }
        public int FtpPort { get; set; }
        public string? FtpUsername { get; set; }
        public string? FtpPassword { get; set; }
        public bool LiveStatusEnabled { get; set; }
        public string? LiveTitle { get; set; }
        public string? LiveMap { get; set; }
        public string? LiveMod { get; set; }
        public int LiveMaxPlayers { get; set; }
        public int LiveCurrentPlayers { get; set; }
        public DateTime LiveLastUpdated { get; set; }
        public bool ShowOnBannerServerList { get; set; }
        public int BannerServerListPosition { get; set; }
        public bool ShowOnPortalServerList { get; set; }
        public bool ShowChatLog { get; set; }
        public string? RconPassword { get; set; }
    }
}