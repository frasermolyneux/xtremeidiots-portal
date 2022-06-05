using Newtonsoft.Json;

using System.Text.Json.Serialization;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players
{
    public class PlayerDto
    {
        [JsonProperty]
        public Guid Id { get; set; }

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; set; }

        [JsonProperty]
        public string Username { get; set; } = string.Empty;

        [JsonProperty]
        public string Guid { get; set; } = string.Empty;

        [JsonProperty]
        public DateTime FirstSeen { get; set; }

        [JsonProperty]
        public DateTime LastSeen { get; set; }

        [JsonProperty]
        public string IpAddress { get; set; } = string.Empty;

        [JsonProperty]
        public List<AliasDto> AliasDtos { get; set; } = new List<AliasDto>();

        [JsonProperty]
        public List<IpAddressDto> IpAddressDtos { get; set; } = new List<IpAddressDto>();

        [JsonProperty]
        public List<AdminActionDto> AdminActionDtos { get; set; } = new List<AdminActionDto>();

        [JsonProperty]
        public List<RelatedPlayerDto> RelatedPlayerDtos { get; set; } = new List<RelatedPlayerDto>();
    }
}