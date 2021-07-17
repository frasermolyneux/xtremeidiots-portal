using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace XI.Portal.Repository.Interfaces
{
    public interface IAppDataRepository
    {
        CloudTable MapsTable { get; }
        CloudTable MapVotesTable { get; }
        Task CreateTablesIfNotExist();
        Task<Tuple<bool, string>> HealthCheck();
    }
}