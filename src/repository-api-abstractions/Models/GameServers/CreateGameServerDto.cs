using Newtonsoft.Json;
using System.Text.Json.Serialization;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers
{
    public class CreateGameServerDto
    {
        public CreateGameServerDto(string title, GameType gameType, string hostname, int queryPort)
        {
            Title = title;
            GameType = gameType;
            Hostname = hostname;
            QueryPort = queryPort;
        }

        [JsonProperty]
        public string Title { get; set; }
        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; set; }
        [JsonProperty]
        public string Hostname { get; set; }
        [JsonProperty]
        public int QueryPort { get; set; }

        [JsonProperty]
        public string? FtpHostname { get; set; }
        [JsonProperty]
        public int FtpPort { get; set; }
        [JsonProperty]
        public string? FtpUsername { get; set; }
        [JsonProperty]
        public string? FtpPassword { get; set; }
        [JsonProperty]
        public string? RconPassword { get; set; }

        [JsonProperty]
        public bool LiveStatusEnabled { get; set; }
        [JsonProperty]
        public int BannerServerListPosition { get; set; }
        [JsonProperty]
        public bool ShowOnBannerServerList { get; set; }
        [JsonProperty]
        public bool ShowOnPortalServerList { get; set; }
        [JsonProperty]
        public bool ShowChatLog { get; set; }

        [JsonProperty]
        public string? HtmlBanner { get; set; }
    }
}
