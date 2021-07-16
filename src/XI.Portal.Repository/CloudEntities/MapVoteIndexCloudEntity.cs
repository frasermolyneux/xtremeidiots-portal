using FM.AzureTableExtensions.Library;
using XI.CommonTypes;

namespace XI.Portal.Repository.CloudEntities
{
    public class MapVoteIndexCloudEntity : TableEntityExtended
    {
        public MapVoteIndexCloudEntity()
        {
        }

        public MapVoteIndexCloudEntity(GameType gameType, string mapName, int totalVotes, int positiveVotes, int negativeVotes)
        {
            PartitionKey = gameType.ToString();
            RowKey = mapName;

            TotalVotes = totalVotes;
            PositiveVotes = positiveVotes;
            NegativeVotes = negativeVotes;
        }

        public int TotalVotes { get; set; }
        public int PositiveVotes { get; set; }
        public int NegativeVotes { get; set; }
    }
}