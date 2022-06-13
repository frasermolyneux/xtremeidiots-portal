using Newtonsoft.Json;

using System.Text.Json.Serialization;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.BanFileMonitors
{
    public class CreateBanFileMonitorDto
    {
        public CreateBanFileMonitorDto(Guid gameServerId, string filePath, GameType gameType)
        {
            GameServerId = gameServerId;
            FilePath = filePath;
            GameType = gameType;
        }

        [JsonProperty]
        public string FilePath { get; private set; }

        [JsonProperty]
        public Guid GameServerId { get; private set; }

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; private set; }
    }
}
