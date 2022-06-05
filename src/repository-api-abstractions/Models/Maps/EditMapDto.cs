using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Maps
{
    public class EditMapDto
    {
        public EditMapDto(Guid mapId)
        {
            MapId = mapId;
        }

        [JsonProperty]
        public Guid MapId { get; private set; }

        [JsonProperty]
        public List<MapFileDto> MapFiles { get; set; } = new List<MapFileDto>();
    }
}
