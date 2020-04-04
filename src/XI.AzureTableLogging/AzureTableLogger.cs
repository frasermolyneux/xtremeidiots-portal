using System;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;

namespace XI.AzureTableLogging
{
    public class AzureTableLogger : ILogger
    {
        private readonly CloudTable _loggingTable;

        public AzureTableLogger(AzureTableLoggerOptions options)
        {
            var storageAccount = CloudStorageAccount.Parse(options.ConnectionString);
            var cloudTableClient = storageAccount.CreateCloudTableClient();

            _loggingTable = cloudTableClient.GetTableReference(options.LogTableName);

            if (options.CreateTableIfNotExists)
                _loggingTable.CreateIfNotExistsAsync();
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
                PartitionKey = DateTime.Now.ToString("yyyyMMdd"),
                RowKey = Guid.NewGuid().ToString()
            };

            var insertOp = TableOperation.Insert(log);
            _loggingTable.ExecuteAsync(insertOp).GetAwaiter().GetResult();
        }
    }
}