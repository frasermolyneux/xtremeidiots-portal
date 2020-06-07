using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;

namespace XI.Portal.Servers.Repository
{
    public class LogFileMonitorStateRepository : ILogFileMonitorStateRepository
    {
        private readonly CloudTable _stateTable;

        public LogFileMonitorStateRepository(ILogFileMonitorStateRepositoryOptions options)
        {
            var storageAccount = CloudStorageAccount.Parse(options.StorageConnectionString);
            var cloudTableClient = storageAccount.CreateCloudTableClient();

            _stateTable = cloudTableClient.GetTableReference(options.StorageTableName);
            _stateTable.CreateIfNotExists();
        }

        public async Task<List<LogFileMonitorStateDto>> GetLogFileMonitorStates()
        {
            var query = new TableQuery<LogFileMonitorStateEntity>().AsTableQuery();

            var results = new List<LogFileMonitorStateDto>();

            TableContinuationToken continuationToken = null;
            do
            {
                var queryResult = await _stateTable.ExecuteQuerySegmentedAsync(query, continuationToken);
                foreach (var entity in queryResult)
                {
                    var fileMonitorStateDto = new LogFileMonitorStateDto
                    {
                        FileMonitorId = Guid.Parse(entity.RowKey),
                        ServerId = entity.ServerId,
                        GameType = entity.GameType,
                        ServerTitle = entity.ServerTitle,
                        FilePath = entity.FilePath,
                        FtpHostname = entity.FtpHostname,
                        FtpUsername = entity.FtpUsername,
                        FtpPassword = entity.FtpPassword,
                        RemoteSize = entity.RemoteSize,
                        LastReadAttempt = entity.LastReadAttempt,
                        LastRead = entity.LastRead
                    };

                    results.Add(fileMonitorStateDto);
                }

                continuationToken = queryResult.ContinuationToken;
            } while (continuationToken != null);

            return results;
        }

        public async Task UpdateState(LogFileMonitorStateDto model)
        {
            try
            {
                var gameServerStatusEntity = new LogFileMonitorStateEntity(model);

                var operation = TableOperation.InsertOrMerge(gameServerStatusEntity);
                await _stateTable.ExecuteAsync(operation);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
    }
}