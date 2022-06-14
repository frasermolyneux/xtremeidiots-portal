using Newtonsoft.Json;

using System.Text.Json.Serialization;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Reports;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players
{
    public class PlayerDto
    {
        [JsonProperty]
        public Guid PlayerId { get; internal set; }

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; internal set; }

        [JsonProperty]
        public string Username { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Guid { get; internal set; } = string.Empty;

        [JsonProperty]
        public DateTime FirstSeen { get; internal set; }

        [JsonProperty]
        public DateTime LastSeen { get; internal set; }

        [JsonProperty]
        public string IpAddress { get; internal set; } = string.Empty;

        [JsonProperty]
        public List<AliasDto> PlayerAliases { get; internal set; } = new List<AliasDto>();

        [JsonProperty]
        public List<IpAddressDto> PlayerIpAddresses { get; internal set; } = new List<IpAddressDto>();

        [JsonProperty]
        public List<AdminActionDto> AdminActions { get; internal set; } = new List<AdminActionDto>();

        [JsonProperty]
        public List<ReportDto> Reports { get; internal set; } = new List<ReportDto>();

        [JsonProperty]
        public List<RelatedPlayerDto> RelatedPlayers { get; internal set; } = new List<RelatedPlayerDto>();
    }
}