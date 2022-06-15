using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Maps
{
    public class UpsertMapVoteDto
    {
        public UpsertMapVoteDto(Guid mapId, Guid playerId, Guid gameServerId, bool like)
        {
            MapId = mapId;
            PlayerId = playerId;
            GameServerId = gameServerId;
            Like = like;
        }

        [JsonProperty]
        public Guid MapId { get; private set; }

        [JsonProperty]
        public Guid PlayerId { get; private set; }

        [JsonProperty]
        public Guid GameServerId { get; private set; }

        [JsonProperty]
        public bool Like { get; private set; }
    }
}
