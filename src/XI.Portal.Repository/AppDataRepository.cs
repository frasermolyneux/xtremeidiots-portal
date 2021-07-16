using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Options;
using XI.Portal.Repository.Config;
using XI.Portal.Repository.Interfaces;

namespace XI.Portal.Repository
{
    public class AppDataRepository : IAppDataRepository
    {
        private readonly CloudTableClient _cloudTableClient;

        public AppDataRepository(IOptions<AppDataOptions> options)
        {
            var storageAccount = CloudStorageAccount.Parse(options.Value.StorageConnectionString);
            _cloudTableClient = storageAccount.CreateCloudTableClient();

            MapVotesTable = _cloudTableClient.GetTableReference(options.Value.MapVotesTableName);
            MapVotesIndexTable = _cloudTableClient.GetTableReference(options.Value.MapVotesIndexTableName);
        }

        public CloudTable MapVotesTable { get; }
        public CloudTable MapVotesIndexTable { get; }

        public async Task CreateTablesIfNotExist()
        {
            await MapVotesTable.CreateIfNotExistsAsync();
            await MapVotesIndexTable.CreateIfNotExistsAsync();
        }

        public async Task<Tuple<bool, string>> HealthCheck()
        {
            try
            {
                _ = await _cloudTableClient.GetServicePropertiesAsync();
                return new Tuple<bool, string>(true, "OK");
            }
            catch (Exception ex)
            {
                return new Tuple<bool, string>(false, ex.Message);
            }
        }
    }
}