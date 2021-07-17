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

            MapsTable = _cloudTableClient.GetTableReference(options.Value.MapsTableName);
            MapVotesTable = _cloudTableClient.GetTableReference(options.Value.MapVotesTableName);
        }

        public CloudTable MapsTable { get; }
        public CloudTable MapVotesTable { get; }

        public async Task CreateTablesIfNotExist()
        {
            await MapsTable.CreateIfNotExistsAsync();
            await MapVotesTable.CreateIfNotExistsAsync();
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