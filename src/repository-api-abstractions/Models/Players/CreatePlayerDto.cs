using Newtonsoft.Json;
using System.Text.Json.Serialization;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players
{
    public class CreatePlayerDto
    {
        public CreatePlayerDto(string username, string guid, GameType gameType)
        {
            Username = username;
            Guid = guid;
            GameType = gameType;
        }

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; private set; }

        [JsonProperty]
        public string Username { get; private set; }

        [JsonProperty]
        public string Guid { get; private set; }

        [JsonProperty]
        public string? IpAddress { get; set; }
    }
}
