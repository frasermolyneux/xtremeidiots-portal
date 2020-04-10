using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using XI.Portal.Demos.Configuration;
using XI.Portal.Demos.Models;

namespace XI.Portal.Demos.Repository
{
    public class DemosRepository : IDemosRepository
    {
        private readonly CloudTable _demosTable;
        private readonly IDemosRepositoryOptions _options;

        public DemosRepository(IDemosRepositoryOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            var storageAccount = CloudStorageAccount.Parse(options.StorageConnectionString);
            var cloudTableClient = storageAccount.CreateCloudTableClient();

            _demosTable = cloudTableClient.GetTableReference(options.StorageTableName);
            _demosTable.CreateIfNotExists();
        }

        public async Task<IDemoDto> GetUserDemo(string userId, string demoId)
        {
            var tableOperation = TableOperation.Retrieve<DemoEntity>(userId, demoId);
            var result = await _demosTable.ExecuteAsync(tableOperation);

            if (result.HttpStatusCode == 404)
                return null;

            var demoDto = (IDemoDto) result.Result;
            return demoDto;
        }

        public async Task UpdateDemo(IDemoDto demoDto)
        {
            var demoEntity = new DemoEntity
            {
                RowKey = demoDto.RowKey,
                UserId = demoDto.UserId,
                Game = demoDto.Game,
                Name = demoDto.Name,
                Created = demoDto.Created,
                Map = demoDto.Map,
                Mod = demoDto.Mod,
                Server = demoDto.Server,
                Size = demoDto.Size
            };

            if (string.IsNullOrWhiteSpace(demoEntity.RowKey)) demoEntity.RowKey = Guid.NewGuid().ToString();

            demoEntity.PartitionKey = demoDto.UserId;

            var operation = TableOperation.InsertOrMerge(demoEntity);
            await _demosTable.ExecuteAsync(operation);
        }
    }
}