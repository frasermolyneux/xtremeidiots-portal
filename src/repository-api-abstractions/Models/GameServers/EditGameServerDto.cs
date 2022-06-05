using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers
{
    public class EditGameServerDto
    {
        public EditGameServerDto(Guid id)
        {
            Id = id;
        }

        [JsonProperty]
        public Guid Id { get; private set; }

        [JsonProperty]
        public string? Title { get; set; }

        [JsonProperty]
        public string? HtmlBanner { get; set; }

        [JsonProperty]
        public string? Hostname { get; set; }

        [JsonProperty]
        public int? QueryPort { get; set; }

        [JsonProperty]
        public string? FtpHostname { get; set; }

        [JsonProperty]
        public int? FtpPort { get; set; }

        [JsonProperty]
        public string? FtpUsername { get; set; }

        [JsonProperty]
        public string? FtpPassword { get; set; }

        [JsonProperty]
        public bool? LiveStatusEnabled { get; set; }

        [JsonProperty]
        public string? LiveTitle { get; set; }

        [JsonProperty]
        public string? LiveMap { get; set; }

        [JsonProperty]
        public string? LiveMod { get; set; }

        [JsonProperty]
        public int? LiveMaxPlayers { get; set; }

        [JsonProperty]
        public int? LiveCurrentPlayers { get; set; }

        [JsonProperty]
        public DateTime? LiveLastUpdated { get; set; }

        [JsonProperty]
        public bool? ShowOnBannerServerList { get; set; }

        [JsonProperty]
        public int? BannerServerListPosition { get; set; }

        [JsonProperty]
        public bool? ShowOnPortalServerList { get; set; }

        [JsonProperty]
        public bool? ShowChatLog { get; set; }

        [JsonProperty]
        public string? RconPassword { get; set; }
    }
}
