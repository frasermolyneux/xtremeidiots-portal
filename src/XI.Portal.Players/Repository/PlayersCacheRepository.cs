using FM.AzureTableExtensions.Library.Extensions;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using XI.Portal.Players.Interfaces;
using XI.Portal.Players.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XI.Portal.Players.Repository
{
    public class PlayersCacheRepository : IPlayersCacheRepository
    {
        private readonly ILogger<PlayersCacheRepository> _logger;
        private readonly CloudTable _playersCache;

        public PlayersCacheRepository(
            ILogger<PlayersCacheRepository> logger,
            IPlayersCacheRepositoryOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var storageAccount = CloudStorageAccount.Parse(options.StorageConnectionString);
            var cloudTableClient = storageAccount.CreateCloudTableClient();

            _playersCache = cloudTableClient.GetTableReference(options.StorageTableName);
            _playersCache.CreateIfNotExistsAsync().Wait();
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
            foreach (var gameType in Enum.GetValues(typeof(GameType)))
            {
                var query = new TableQuery<TableEntity>()
                    .Where(TableQuery.CombineFilters(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, gameType.ToString()), TableOperators.And,
                        TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThan, DateTime.UtcNow.AddHours(-1))))
                    .Select(new[] { "PartitionKey", "RowKey" });

                TableContinuationToken continuationToken = null;
                do
                {
                    var entries = await _playersCache.ExecuteQuerySegmentedAsync(query, continuationToken);

                    continuationToken = entries.ContinuationToken;

                    var deleteBatches = entries.Batch(100);

                    foreach (var deleteBatch in deleteBatches)
                    {
                        var batchOperation = new TableBatchOperation();

                        foreach (var entity in deleteBatch) batchOperation.Add(TableOperation.Delete(entity));

                        await _playersCache.ExecuteBatchAsync(batchOperation);
                    }
                } while (continuationToken != null);
            }
        }
    }
}