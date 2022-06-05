using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players
{
    public class EditPlayerDto
    {
        public EditPlayerDto(Guid id)
        {
            Id = id;
        }

        [JsonProperty]
        public Guid Id { get; set; }

        [JsonProperty]
        public string? Username { get; set; }

        [JsonProperty]
        public string? IpAddress { get; set; }
    }
}
