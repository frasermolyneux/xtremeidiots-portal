using System;
using Microsoft.Azure.Cosmos.Table;
using XI.Portal.Demos.Configuration;

namespace XI.Portal.Demos.Repository
{
    public class DemosRepository : IDemosRepository
    {
        private readonly IDemosRepositoryOptions _options;
        private readonly CloudTable _demosTable;

        public DemosRepository(IDemosRepositoryOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            var storageAccount = CloudStorageAccount.Parse(options.StorageConnectionString);
            var cloudTableClient = storageAccount.CreateCloudTableClient();

            _demosTable = cloudTableClient.GetTableReference(options.StorageTableName);
            _demosTable.CreateIfNotExists();
        }
    }
}