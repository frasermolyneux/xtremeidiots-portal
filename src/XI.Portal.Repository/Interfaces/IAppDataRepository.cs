using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace XI.Portal.Repository.Interfaces
{
    public interface IAppDataRepository
    {
        CloudTable MapVotesTable { get; }
        CloudTable MapVotesIndexTable { get; }
        Task CreateTablesIfNotExist();
        Task<Tuple<bool, string>> HealthCheck();
    }
}