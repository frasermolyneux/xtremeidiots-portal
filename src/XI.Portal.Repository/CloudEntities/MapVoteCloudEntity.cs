using FM.AzureTableExtensions.Library;
using XI.Portal.Repository.Dtos;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Portal.Repository.CloudEntities
{
    public class MapVoteCloudEntity : TableEntityExtended
    {
        public MapVoteCloudEntity()
        {
        }

        public MapVoteCloudEntity(GameType gameType, string mapName, string playerGuid, bool like)
        {
            PartitionKey = gameType.ToString();
            RowKey = $"{mapName}-{playerGuid}";

            MapName = mapName;
            Guid = playerGuid;
            Like = like;
        }

        public MapVoteCloudEntity(LegacyMapVoteDto mapVoteDto)
        {
            PartitionKey = mapVoteDto.GameType.ToString();
            RowKey = $"{mapVoteDto.MapName}-{mapVoteDto.Guid}";

            MapName = mapVoteDto.MapName;
            Guid = mapVoteDto.Guid;
            Like = mapVoteDto.Like;
        }

        public string MapName { get; set; }
        public string Guid { get; set; }
        public bool Like { get; set; }
    }
}