using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers
{
    public class CreateGameServerStatDto
    {
        public CreateGameServerStatDto(Guid gameServerId, int playerCount, string mapName)
        {
            GameServerId = gameServerId;
            PlayerCount = playerCount;
            MapName = mapName;
        }

        [JsonProperty]
        public Guid GameServerId { get; private set; }

        [JsonProperty]
        public int PlayerCount { get; private set; }

        [JsonProperty]
        public string MapName { get; private set; }

    }
}
