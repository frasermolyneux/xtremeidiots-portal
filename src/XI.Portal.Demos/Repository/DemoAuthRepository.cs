using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using XI.Portal.Demos.Configuration;
using XI.Portal.Demos.Models;

namespace XI.Portal.Demos.Repository
{
    public class DemoAuthRepository : IDemoAuthRepository
    {
        private readonly IDemoAuthRepositoryOptions _options;
        private readonly CloudTable _demoAuthTable;

        public DemoAuthRepository(IDemoAuthRepositoryOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            var storageAccount = CloudStorageAccount.Parse(options.StorageConnectionString);
            var cloudTableClient = storageAccount.CreateCloudTableClient();

            _demoAuthTable = cloudTableClient.GetTableReference(options.StorageTableName);
            _demoAuthTable.CreateIfNotExists();
        }

        public async Task<string> GetAuthKey(string userId)
        {
            var tableOperation = TableOperation.Retrieve<DemoAuthEntity>(userId.First().ToString(), userId);
            var result = await _demoAuthTable.ExecuteAsync(tableOperation);

            if (result.HttpStatusCode == 404)
                return null;

            var demoAuthEntity = (DemoAuthEntity) result.Result;
            return demoAuthEntity.AuthKey;
        }

        public async Task UpdateAuthKey(string userId, string authKey)
        {
            var demoAuthEntity = new DemoAuthEntity
            {
                AuthKey = authKey,
                PartitionKey = userId.First().ToString(),
                RowKey = userId
            };

            var insertOp = TableOperation.InsertOrMerge(demoAuthEntity);
            await _demoAuthTable.ExecuteAsync(insertOp);
        }
    }
}