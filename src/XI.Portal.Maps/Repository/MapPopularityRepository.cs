using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using XI.CommonTypes;
using XI.Portal.Maps.Dto;
using XI.Portal.Maps.Interfaces;
using XI.Portal.Maps.Models;

namespace XI.Portal.Maps.Repository
{
    public class MapPopularityRepository : IMapPopularityRepository
    {
        private readonly CloudTable _mapPopularityTable;

        public MapPopularityRepository(IMapPopularityRepositoryOptions options)
        {
            var storageAccount = CloudStorageAccount.Parse(options.StorageConnectionString);
            var cloudTableClient = storageAccount.CreateCloudTableClient();

            _mapPopularityTable = cloudTableClient.GetTableReference(options.StorageTableName);
            _mapPopularityTable.CreateIfNotExists();
        }

        public async Task<MapPopularityDto> GetMapPopularity(GameType gameType, string mapName)
        {
            var tableOperation = TableOperation.Retrieve<MapPopularityEntity>(gameType.ToString(), mapName);
            var result = await _mapPopularityTable.ExecuteAsync(tableOperation);

            if (result.HttpStatusCode == 404)
                return null;

            var mapPopularity = (MapPopularityEntity) result.Result;

            var mapPopularityDto = new MapPopularityDto
            {
                GameType = gameType,
                MapName = mapPopularity.RowKey,
                MapVotes = mapPopularity.MapVotes
            };

            return mapPopularityDto;
        }

        public async Task UpdateMapPopularity(MapPopularityDto mapPopularityDto)
        {
            var mapPopularityEntity = new MapPopularityEntity(mapPopularityDto);

            var operation = TableOperation.InsertOrMerge(mapPopularityEntity);
            await _mapPopularityTable.ExecuteAsync(operation);
        }
    }
}