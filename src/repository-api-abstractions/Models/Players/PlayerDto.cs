using Newtonsoft.Json;

using System.Text.Json.Serialization;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players
{
    public class PlayerDto
    {
        [JsonProperty]
        public Guid Id { get; internal set; }

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
        public List<AliasDto> AliasDtos { get; internal set; } = new List<AliasDto>();

        [JsonProperty]
        public List<IpAddressDto> IpAddressDtos { get; internal set; } = new List<IpAddressDto>();

        [JsonProperty]
        public List<AdminActionDto> AdminActionDtos { get; internal set; } = new List<AdminActionDto>();

        [JsonProperty]
        public List<RelatedPlayerDto> RelatedPlayerDtos { get; internal set; } = new List<RelatedPlayerDto>();
    }
}