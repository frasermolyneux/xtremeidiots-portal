using System;
using System.Threading.Tasks;
using FM.AzureTableExtensions.Library.Extensions;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using XI.AzureTableLogging.Interfaces;
using XI.AzureTableLogging.Models;

namespace XI.AzureTableLogging.Logger
{
    public class AzureTableLogger : ILogger
    {
        private readonly CloudTable _loggingTable;

        public AzureTableLogger(IAzureTableLoggerOptions options)
        {
            var storageAccount = CloudStorageAccount.Parse(options.StorageConnectionString);
            var cloudTableClient = storageAccount.CreateCloudTableClient();

            _loggingTable = cloudTableClient.GetTableReference(options.StorageTableName);

            if (options.CreateTableIfNotExists)
                _loggingTable.CreateIfNotExistsAsync().Wait();
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            var log = new LogEntity
            {
                EventId = eventId.ToString(),
                LogLevel = logLevel.ToString(),
                Message = formatter(state, exception),
                Exception = exception,
                PartitionKey = DateTime.UtcNow.ToString("yyyyMMdd"),
                HourKey = DateTime.UtcNow.ToString("HH"),
                RowKey = Guid.NewGuid().ToString()
            };

            var insertOp = TableOperation.Insert(log);
            _loggingTable.ExecuteAsync(insertOp).GetAwaiter().GetResult();
        }

        public async Task RemoveOldEntries(int daysToKeep)
        {
            var dateOffset = DateTime.UtcNow.AddDays(-daysToKeep);
            for (var i = 0; i < 7; i++)
            {
                var query = new TableQuery<TableEntity>()
                    .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, dateOffset.ToString("yyyyMMdd")))
                    .Select(new[] {"PartitionKey", "RowKey"});

                TableContinuationToken continuationToken = null;
                do
                {
                    var entries = await _loggingTable.ExecuteQuerySegmentedAsync(query, continuationToken);

                    continuationToken = entries.ContinuationToken;

                    var deleteBatches = entries.Batch(100);

                    foreach (var deleteBatch in deleteBatches)
                    {
                        var batchOperation = new TableBatchOperation();

                        foreach (var entity in deleteBatch) batchOperation.Add(TableOperation.Delete(entity));

                        await _loggingTable.ExecuteBatchAsync(batchOperation);
                    }
                } while (continuationToken != null);

                dateOffset = dateOffset.AddDays(-1);
            }
        }
    }
}