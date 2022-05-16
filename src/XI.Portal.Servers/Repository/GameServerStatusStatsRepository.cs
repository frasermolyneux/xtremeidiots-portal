﻿using FM.AzureTableExtensions.Library.Extensions;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Extensions;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;

namespace XI.Portal.Servers.Repository
{
    public class GameServerStatusStatsRepository : IGameServerStatusStatsRepository
    {
        private readonly CloudTable _statsTable;

        public GameServerStatusStatsRepository(IGameServerStatusStatsRepositoryOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            var storageAccount = CloudStorageAccount.Parse(options.StorageConnectionString);
            var cloudTableClient = storageAccount.CreateCloudTableClient();

            _statsTable = cloudTableClient.GetTableReference(options.StorageTableName);
            _statsTable.CreateIfNotExistsAsync().Wait();
        }

        public async Task UpdateEntry(GameServerStatusStatsDto model)
        {
            var gameServerStatusStats = new GameServerStatusStatsEntity
            {
                PartitionKey = model.ServerId.ToString(),
                GameType = model.GameType,
                PlayerCount = model.PlayerCount,
                MapName = model.MapName
            };

            if (string.IsNullOrWhiteSpace(gameServerStatusStats.RowKey)) gameServerStatusStats.RowKey = Guid.NewGuid().ToString();

            var operation = TableOperation.InsertOrMerge(gameServerStatusStats);
            await _statsTable.ExecuteAsync(operation);
        }

        public async Task<List<GameServerStatusStatsDto>> GetGameServerStatusStats(GameServerStatusStatsFilterModel filterModel)
        {
            if (filterModel == null) throw new NullReferenceException(nameof(filterModel));

            var query = new TableQuery<GameServerStatusStatsEntity>().ApplyFilter(filterModel);

            var results = new List<GameServerStatusStatsDto>();

            TableContinuationToken continuationToken = null;
            do
            {
                var queryResult = await _statsTable.ExecuteQuerySegmentedAsync(query, continuationToken);
                foreach (var entity in queryResult)
                {
                    var gameServerStatusStatsDto = new GameServerStatusStatsDto
                    {
                        ServerId = Guid.Parse(entity.PartitionKey),
                        GameType = entity.GameType,
                        PlayerCount = entity.PlayerCount,
                        MapName = entity.MapName,
                        Timestamp = entity.Timestamp
                    };

                    results.Add(gameServerStatusStatsDto);
                }

                continuationToken = queryResult.ContinuationToken;
            } while (continuationToken != null);

            switch (filterModel.Order)
            {
                case GameServerStatusStatsFilterModel.OrderBy.TimestampAsc:
                    results = results.OrderBy(s => s.Timestamp).ToList();
                    break;
                case GameServerStatusStatsFilterModel.OrderBy.TimestampDesc:
                    results = results.OrderByDescending(s => s.Timestamp).ToList();
                    break;
            }

            return results;
        }

        public async Task DeleteGameServerStatusStats(Guid serverId)
        {
            var query = new TableQuery<GameServerStatusStatsEntity>()
                .Where(TableQuery.GenerateFilterCondition("ServerId", QueryComparisons.Equal, serverId.ToString()));

            TableContinuationToken continuationToken = null;
            do
            {
                var queryResult = await _statsTable.ExecuteQuerySegmentedAsync(query, continuationToken);
                foreach (var entity in queryResult)
                {
                    var deleteOperation = TableOperation.Delete(entity);
                    await _statsTable.ExecuteAsync(deleteOperation);
                }

                continuationToken = queryResult.ContinuationToken;
            } while (continuationToken != null);
        }

        public async Task RemoveOldEntries(List<Guid> serverIds)
        {
            foreach (var serverId in serverIds)
            {
                var query = new TableQuery<TableEntity>()
                    .Where(TableQuery.CombineFilters(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, serverId.ToString()), TableOperators.And,
                        TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThan, DateTime.UtcNow.AddMonths(-1))))
                    .Select(new[] { "PartitionKey", "RowKey" });

                TableContinuationToken continuationToken = null;
                do
                {
                    var entries = await _statsTable.ExecuteQuerySegmentedAsync(query, continuationToken);

                    continuationToken = entries.ContinuationToken;

                    var deleteBatches = entries.Batch(100);

                    foreach (var deleteBatch in deleteBatches)
                    {
                        var batchOperation = new TableBatchOperation();

                        foreach (var entity in deleteBatch) batchOperation.Add(TableOperation.Delete(entity));

                        await _statsTable.ExecuteBatchAsync(batchOperation);
                    }
                } while (continuationToken != null);
            }
        }
    }
}