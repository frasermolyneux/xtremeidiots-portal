using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players
{
    public class EditPlayerDto
    {
        public EditPlayerDto(Guid playerId)
        {
            PlayerId = playerId;
        }

        [JsonProperty]
        public Guid PlayerId { get; set; }

        [JsonProperty]
        public string? Username { get; set; }

        [JsonProperty]
        public string? IpAddress { get; set; }
    }
}
