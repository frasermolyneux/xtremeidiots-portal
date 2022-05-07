using System;
using System.Text.Json.Serialization;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models
{
    public class GameServerDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; set; }
        public string Hostname { get; set; } = string.Empty;
        public int QueryPort { get; set; }
        public string FtpHostname { get; set; } = string.Empty;
        public string FtpUsername { get; set; } = string.Empty;
        public string FtpPassword { get; set; } = string.Empty;
        public string RconPassword { get; set; } = string.Empty;
        public bool ShowOnBannerServerList { get; set; }
        public string? HtmlBanner { get; set; }
        public int BannerServerListPosition { get; set; }
        public bool ShowOnPortalServerList { get; set; }
        public bool ShowChatLog { get; set; }
    }
}