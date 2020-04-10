using System;
using System.Linq;
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

            var demoAuthEntity = (IDemoDto) result.Result;
            return demoAuthEntity;
        }

        public async Task UpdateDemo(IDemoDto demo)
        {
            var demoEntity = new DemoEntity
            {
                RowKey = demo.RowKey,
                UserId = demo.UserId,
                Game = demo.Game,
                Name = demo.Name, 
                Created = demo.Created,
                Map = demo.Map,
                Mod = demo.Mod,
                Server = demo.Server,
                Size = demo.Size
            };

            if (string.IsNullOrWhiteSpace(demoEntity.RowKey)) demoEntity.RowKey = Guid.NewGuid().ToString();

            demoEntity.PartitionKey = demo.UserId;

            var insertOp = TableOperation.InsertOrMerge(demoEntity);
            await _demosTable.ExecuteAsync(insertOp);
        }
    }
}