using Newtonsoft.Json;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Maps
{
    public class MapVoteDto
    {
        [JsonProperty]
        public Guid MapVoteId { get; internal set; }

        [JsonProperty]
        public Guid MapId { get; internal set; }

        [JsonProperty]
        public Guid PlayerId { get; internal set; }

        [JsonProperty]
        public Guid? GameServerId { get; internal set; }

        [JsonProperty]
        public bool Like { get; internal set; }

        [JsonProperty]
        public DateTime Timestamp { get; internal set; }

        [JsonProperty]
        public GameServerDto? GameServer { get; internal set; }

        [JsonProperty]
        public MapDto? Map { get; internal set; }

        [JsonProperty]
        public PlayerDto? Player { get; internal set; }
    }
}
