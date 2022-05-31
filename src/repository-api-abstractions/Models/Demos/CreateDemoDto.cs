using Newtonsoft.Json;
using System.Text.Json.Serialization;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Demos
{
    public class CreateDemoDto
    {
        public CreateDemoDto(GameType gameType, string userId)
        {
            Game = gameType;
            UserId = userId;
        }

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType Game { get; set; }
        [JsonProperty]
        public string UserId { get; set; }
    }
}
