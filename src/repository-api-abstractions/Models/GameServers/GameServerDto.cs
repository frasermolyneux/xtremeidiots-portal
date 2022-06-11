using Newtonsoft.Json;

using System.Text.Json.Serialization;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.BanFileMonitors;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers
{
    public class GameServerDto
    {
        [JsonProperty]
        public Guid Id { get; internal set; }

        [JsonProperty]
        public string? Title { get; internal set; }

        [JsonProperty]
        public string? HtmlBanner { get; internal set; }

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; internal set; }

        [JsonProperty]
        public string? Hostname { get; internal set; }

        [JsonProperty]
        public int QueryPort { get; internal set; }

        [JsonProperty]
        public string? FtpHostname { get; internal set; }

        [JsonProperty]
        public int FtpPort { get; internal set; }

        [JsonProperty]
        public string? FtpUsername { get; internal set; }

        [JsonProperty]
        public string? FtpPassword { get; internal set; }

        [JsonProperty]
        public bool LiveStatusEnabled { get; internal set; }

        [JsonProperty]
        public string? LiveTitle { get; internal set; }

        [JsonProperty]
        public string? LiveMap { get; internal set; }

        [JsonProperty]
        public string? LiveMod { get; internal set; }

        [JsonProperty]
        public int LiveMaxPlayers { get; internal set; }

        [JsonProperty]
        public int LiveCurrentPlayers { get; internal set; }

        [JsonProperty]
        public DateTime LiveLastUpdated { get; internal set; }

        [JsonProperty]
        public bool ShowOnBannerServerList { get; internal set; }

        [JsonProperty]
        public int BannerServerListPosition { get; internal set; }

        [JsonProperty]
        public bool ShowOnPortalServerList { get; internal set; }

        [JsonProperty]
        public bool ShowChatLog { get; internal set; }

        [JsonProperty]
        public string? RconPassword { get; internal set; }

        [JsonProperty]
        public List<BanFileMonitorDto> BanFileMonitorDtos { get; internal set; } = new List<BanFileMonitorDto>();

        [JsonProperty]
        public List<LivePlayerDto> LivePlayerDtos { get; internal set; } = new List<LivePlayerDto>();

        public void ClearFtpCredentials()
        {
            FtpHostname = null;
            FtpUsername = null;
            FtpPassword = null;
        }

        public void ClearRconCredentials()
        {
            RconPassword = null;
        }
    }
}