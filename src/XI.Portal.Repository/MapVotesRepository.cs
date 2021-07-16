using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Options;
using XI.CommonTypes;
using XI.Portal.Bus.Models;
using XI.Portal.Repository.CloudEntities;
using XI.Portal.Repository.Config;
using XI.Portal.Repository.Interfaces;

namespace XI.Portal.Repository
{
    public class MapVotesRepository : AppDataRepository, IMapVotesRepository
    {
        public MapVotesRepository(IOptions<AppDataOptions> options) : base(options)
        {
        }

        public async Task UpdateMapVote(MapVote mapVote)
        {
            var contactCloudEntity = new MapVoteCloudEntity(mapVote.GameType, mapVote.MapName, mapVote.Guid, mapVote.Like);
            var operation = TableOperation.InsertOrMerge(contactCloudEntity);
            await MapVotesTable.ExecuteAsync(operation);
        }

        public async Task RebuildIndex(Dictionary<GameType, string> maps)
        {
            foreach (var (gameType, mapName) in maps)
            {
                var mapVotesQuery = new TableQuery<MapVoteCloudEntity>()
                    .Where(TableQuery.GenerateFilterCondition(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, gameType.ToString()), TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, mapName)))
                    .Select(new[] {"PartitionKey", "RowKey", "Like"});

                var mapVotes = new List<MapVoteCloudEntity>();

                TableContinuationToken continuationToken = null;
                do
                {
                    var tableQueryResult = await MapVotesTable.ExecuteQuerySegmentedAsync(mapVotesQuery, continuationToken);
                    mapVotes.AddRange(tableQueryResult);
                    continuationToken = tableQueryResult.ContinuationToken;
                } while (continuationToken != null);

                var mapVoteIndexCloudEntity = await GetMapVoteIndexCloudEntity(gameType, mapName);
                if (mapVoteIndexCloudEntity == null) mapVoteIndexCloudEntity = new MapVoteIndexCloudEntity(gameType, mapName, 0, 0, 0);

                mapVoteIndexCloudEntity.TotalVotes = mapVotes.Count;
                mapVoteIndexCloudEntity.PositiveVotes = mapVotes.Count(v => v.Like);
                mapVoteIndexCloudEntity.NegativeVotes = mapVotes.Count(v => !v.Like);

                await UpdateMapVoteIndex(mapVoteIndexCloudEntity);
            }
        }

        private async Task<MapVoteIndexCloudEntity> GetMapVoteIndexCloudEntity(GameType gameType, string mapName)
        {
            var tableOperation = TableOperation.Retrieve<MapVoteIndexCloudEntity>(gameType.ToString(), mapName);
            var result = await MapVotesIndexTable.ExecuteAsync(tableOperation);

            if (result.HttpStatusCode == 404) return null;

            return (MapVoteIndexCloudEntity) result.Result;
        }

        private async Task UpdateMapVoteIndex(MapVoteIndexCloudEntity mapVoteIndexCloudEntity)
        {
            var operation = TableOperation.InsertOrMerge(mapVoteIndexCloudEntity);
            await MapVotesTable.ExecuteAsync(operation);
        }
    }
}