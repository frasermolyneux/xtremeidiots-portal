using FM.AzureTableExtensions.Library.Extensions;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Players.Dto;
using XI.Portal.Players.Interfaces;
using XI.Portal.Players.Models;

namespace XI.Portal.Players.Repository
{
    public class PlayerLocationsRepository : IPlayerLocationsRepository
    {
        private readonly CloudTable _locationsTable;
        private readonly ILogger<IPlayerLocationsRepository> _logger;

        public PlayerLocationsRepository(
            ILogger<IPlayerLocationsRepository> logger,
            IPlayerLocationsRepositoryOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var storageAccount = CloudStorageAccount.Parse(options.StorageConnectionString);
            var cloudTableClient = storageAccount.CreateCloudTableClient();

            _locationsTable = cloudTableClient.GetTableReference(options.StorageTableName);
            _locationsTable.CreateIfNotExistsAsync().Wait();
        }

        public async Task<List<PlayerLocationDto>> GetLocations()
        {
            var query = new TableQuery<PlayerLocationEntity>().AsTableQuery();

            var results = new List<PlayerLocationDto>();

            TableContinuationToken continuationToken = null;
            do
            {
                var queryResult = await _locationsTable.ExecuteQuerySegmentedAsync(query, continuationToken);
                foreach (var entity in queryResult)
                {
                    var playerLocationDto = new PlayerLocationDto
                    {
                        GameType = entity.GameType,
                        ServerId = entity.ServerId,
                        ServerName = entity.ServerName,
                        Guid = entity.Guid,
                        PlayerName = entity.PlayerName,
                        GeoLocation = entity.GeoLocation
                    };

                    results.Add(playerLocationDto);
                }

                continuationToken = queryResult.ContinuationToken;
            } while (continuationToken != null);

            return results;
        }

        public async Task UpdateEntry(PlayerLocationDto model)
        {
            var playerLocationEntity = new PlayerLocationEntity
            {
                PartitionKey = model.ServerId.ToString(),
                GameType = model.GameType,
                ServerId = model.ServerId,
                ServerName = model.ServerName,
                Guid = model.Guid,
                PlayerName = model.PlayerName,
                GeoLocation = model.GeoLocation
            };

            if (string.IsNullOrWhiteSpace(playerLocationEntity.RowKey)) playerLocationEntity.RowKey = GenerateRowKeyForPlayerLocation(model);

            var operation = TableOperation.InsertOrMerge(playerLocationEntity);
            await _locationsTable.ExecuteAsync(operation);
        }

        public async Task RemoveOldEntries(List<Guid> serverIds)
        {
            foreach (var serverId in serverIds)
            {
                var query = new TableQuery<TableEntity>()
                    .Where(TableQuery.CombineFilters(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, serverId.ToString()), TableOperators.And,
                        TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThan, DateTime.UtcNow.AddHours(-24))))
                    .Select(new[] { "PartitionKey", "RowKey" });

                TableContinuationToken continuationToken = null;
                do
                {
                    var entries = await _locationsTable.ExecuteQuerySegmentedAsync(query, continuationToken);

                    continuationToken = entries.ContinuationToken;

                    var deleteBatches = entries.Batch(100);

                    foreach (var deleteBatch in deleteBatches)
                    {
                        var batchOperation = new TableBatchOperation();

                        foreach (var entity in deleteBatch) batchOperation.Add(TableOperation.Delete(entity));

                        await _locationsTable.ExecuteBatchAsync(batchOperation);
                    }
                } while (continuationToken != null);
            }
        }

        private static string GenerateRowKeyForPlayerLocation(PlayerLocationDto playerLocationDto)
        {
            return $"{playerLocationDto.GameType}-{playerLocationDto.ServerId}-{playerLocationDto.Guid}";
        }
    }
}