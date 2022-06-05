using Newtonsoft.Json;

using System.Text.Json.Serialization;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Maps
{
    public class CreateMapDto
    {
        public CreateMapDto(GameType gameType, string mapName)
        {
            GameType = gameType;
            MapName = mapName;
        }

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; set; }

        [JsonProperty]
        public string MapName { get; set; }

        [JsonProperty]
        public List<MapFileDto> MapFiles { get; set; } = new List<MapFileDto>();
    }
}
