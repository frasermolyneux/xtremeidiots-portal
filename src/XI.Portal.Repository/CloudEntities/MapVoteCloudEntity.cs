using FM.AzureTableExtensions.Library;
using XI.CommonTypes;

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

            Guid = playerGuid;
            Like = like;
        }

        public string Guid { get; set; }
        public bool Like { get; set; }
    }
}