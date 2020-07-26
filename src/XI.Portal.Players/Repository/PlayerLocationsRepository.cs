using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Microsoft.Extensions.Logging;
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
            _locationsTable.CreateIfNotExists();
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

        public async Task RemoveOldEntries()
        {
            var query = new TableQuery<PlayerLocationEntity>()
                .Where(TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThan, DateTime.Now.AddHours(-24)));

            TableContinuationToken continuationToken = null;
            do
            {
                var queryResult = await _locationsTable.ExecuteQuerySegmentedAsync(query, continuationToken);

                _logger.LogInformation($"Removing {queryResult.Count()} cached entries from the players location repository");

                foreach (var entity in queryResult)
                {
                    var deleteOperation = TableOperation.Delete(entity);
                    await _locationsTable.ExecuteAsync(deleteOperation);
                }

                continuationToken = queryResult.ContinuationToken;
            } while (continuationToken != null);
        }

        private static string GenerateRowKeyForPlayerLocation(PlayerLocationDto playerLocationDto)
        {
            return $"{playerLocationDto.GameType}-{playerLocationDto.ServerId}-{playerLocationDto.Guid}";
        }
    }
}