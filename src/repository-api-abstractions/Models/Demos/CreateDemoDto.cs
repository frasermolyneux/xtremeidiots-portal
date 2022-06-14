using Newtonsoft.Json;

using System.Text.Json.Serialization;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Demos
{
    public class CreateDemoDto
    {
        public CreateDemoDto(GameType gameType, Guid userProfileId)
        {
            GameType = gameType;
            UserProfileId = userProfileId;
        }

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; private set; }

        [JsonProperty]
        public Guid UserProfileId { get; private set; }
    }
}
