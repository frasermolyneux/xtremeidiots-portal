using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XI.Portal.Repository.CloudEntities;
using XI.Portal.Repository.Config;
using XI.Portal.Repository.Dtos;
using XI.Portal.Repository.Extensions;
using XI.Portal.Repository.Interfaces;
using XI.Portal.Repository.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Portal.Repository
{
    public class MapsRepository : AppDataRepository, IMapsRepository
    {
        public MapsRepository(IOptions<AppDataOptions> options) : base(options)
        {
        }

        public async Task InsertOrMergeMap(MapDto mapDto)
        {
            var cloudEntity = new MapCloudEntity(mapDto);
            var operation = TableOperation.InsertOrMerge(cloudEntity);
            await MapsTable.ExecuteAsync(operation);
        }

        public async Task InsertOrMergeMapVote(MapVoteDto mapVoteDto)
        {
            var cloudEntity = new MapVoteCloudEntity(mapVoteDto);
            var operation = TableOperation.InsertOrMerge(cloudEntity);
            await MapVotesTable.ExecuteAsync(operation);
        }

        public async Task<MapDto> GetMap(GameType gameType, string mapName)
        {
            var tableOperation = TableOperation.Retrieve<MapCloudEntity>(gameType.ToString(), mapName);
            var result = await MapsTable.ExecuteAsync(tableOperation);

            if (result.HttpStatusCode == 404) return null;

            var entity = (MapCloudEntity)result.Result;

            return entity.ToDto();
        }

        public async Task<int> GetMapsCount(MapsQueryOptions queryOptions)
        {
            var maps = await GetMaps(queryOptions);
            return maps.Count;
        }

        public async Task<List<MapDto>> GetMaps(MapsQueryOptions queryOptions)
        {
            var entityResults = new List<MapCloudEntity>();
            var query = new TableQuery<MapCloudEntity>().ApplyQueryOptions(queryOptions);

            TableContinuationToken indexQueryContinuationToken = null;
            do
            {
                var queryResult = await MapsTable.ExecuteQuerySegmentedAsync(query, indexQueryContinuationToken);
                entityResults.AddRange(queryResult);

                indexQueryContinuationToken = queryResult.ContinuationToken;
            } while (indexQueryContinuationToken != null);

            entityResults = entityResults.ApplyQueryOptions(queryOptions);

            return entityResults.Select(e => e.ToDto()).ToList();
        }

        public async Task RebuildMapVotes()
        {
            var maps = await GetMaps(new MapsQueryOptions());
            foreach (var mapDto in maps)
            {
                var mapVotesQuery = new TableQuery<MapVoteCloudEntity>()
                    .Where(TableQuery.CombineFilters(TableQuery.GenerateFilterCondition(nameof(MapVoteCloudEntity.PartitionKey), QueryComparisons.Equal, mapDto.GameType.ToString()), TableOperators.And,
                        TableQuery.GenerateFilterCondition(nameof(MapVoteCloudEntity.MapName), QueryComparisons.Equal, mapDto.MapName)));


                var mapVotes = new List<MapVoteCloudEntity>();
                TableContinuationToken continuationToken = null;
                do
                {
                    var tableQueryResult = await MapVotesTable.ExecuteQuerySegmentedAsync(mapVotesQuery, continuationToken);
                    mapVotes.AddRange(tableQueryResult);
                    continuationToken = tableQueryResult.ContinuationToken;
                } while (continuationToken != null);

                mapDto.PositiveVotes = mapVotes.Count(mv => mv.Like);
                mapDto.NegativeVotes = mapVotes.Count(mv => !mv.Like);
                mapDto.TotalVotes = mapVotes.Count;

                if (mapDto.TotalVotes > 0)
                {
                    mapDto.PositivePercentage = (double)mapDto.PositiveVotes / mapDto.TotalVotes * 100;
                    mapDto.NegativePercentage = (double)mapDto.NegativeVotes / mapDto.TotalVotes * 100;
                }
                else
                {
                    mapDto.PositivePercentage = 0;
                    mapDto.NegativePercentage = 0;
                }


                await InsertOrMergeMap(mapDto);
            }
        }
    }
}