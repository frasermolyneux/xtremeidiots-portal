using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using XI.CommonTypes;
using XI.Portal.Players.Interfaces;
using XI.Portal.Players.Models;

namespace XI.Portal.Players.Repository
{
    public class PlayersCacheRepository : IPlayersCacheRepository
    {
        private readonly CloudTable _playersCache;

        public PlayersCacheRepository(IPlayersCacheRepositoryOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            var storageAccount = CloudStorageAccount.Parse(options.StorageConnectionString);
            var cloudTableClient = storageAccount.CreateCloudTableClient();

            _playersCache = cloudTableClient.GetTableReference(options.StorageTableName);
            _playersCache.CreateIfNotExists();
        }

        public async Task<PlayerCacheEntity> GetPlayer(GameType gameType, string guid)
        {
            var tableOperation = TableOperation.Retrieve<PlayerCacheEntity>(gameType.ToString(), guid);
            var result = await _playersCache.ExecuteAsync(tableOperation);

            if (result.HttpStatusCode == 404)
                return null;

            var playerCacheEntity = (PlayerCacheEntity)result.Result;
            return playerCacheEntity;
        }

        public async Task UpdatePlayer(PlayerCacheEntity model)
        {
            var operation = TableOperation.InsertOrMerge(model);
            await _playersCache.ExecuteAsync(operation);
        }

        public async Task RemoveOldEntries()
        {
            var query = new TableQuery<PlayerCacheEntity>()
                .Where(TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThan, DateTime.Now.AddHours(-1)));

            TableContinuationToken continuationToken = null;
            do
            {
                var queryResult = await _playersCache.ExecuteQuerySegmentedAsync(query, continuationToken);
                foreach (var entity in queryResult)
                {
                    var deleteOperation = TableOperation.Delete(entity);
                    await _playersCache.ExecuteAsync(deleteOperation);
                }

                continuationToken = queryResult.ContinuationToken;
            } while (continuationToken != null);
        }
    }
}