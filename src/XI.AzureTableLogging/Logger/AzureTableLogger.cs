using System;
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
                _loggingTable.CreateIfNotExists();
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
                RowKey = Guid.NewGuid().ToString()
            };

            var insertOp = TableOperation.Insert(log);
            _loggingTable.ExecuteAsync(insertOp).GetAwaiter().GetResult();
        }
    }
}