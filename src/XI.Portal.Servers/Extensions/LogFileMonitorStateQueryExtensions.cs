using System.Linq;
using Microsoft.Azure.Cosmos.Table;
using XI.Portal.Servers.Models;

namespace XI.Portal.Servers.Extensions
{
    public static class LogFileMonitorStateQueryExtensions
    {
        internal static TableQuery<LogFileMonitorStateEntity> ApplyFilter(this TableQuery<LogFileMonitorStateEntity> query, FileMonitorFilterModel filterModel)
        {
            if (filterModel.GameTypes != null)
            {
                var firstGameType = filterModel.GameTypes.First();
                var gameTypesQuery = TableQuery.GenerateFilterCondition(nameof(LogFileMonitorStateEntity.GameType), QueryComparisons.Equal, firstGameType.ToString());

                if (filterModel.GameTypes.Count > 1)
                {
                    foreach (var gameType in filterModel.GameTypes.Skip(1))
                    {
                        var subCondition = TableQuery.GenerateFilterCondition(nameof(LogFileMonitorStateEntity.GameType), QueryComparisons.Equal, gameType.ToString());
                        gameTypesQuery = TableQuery.CombineFilters(gameTypesQuery, TableOperators.Or, subCondition);
                    }
                }

                query = query.Where(gameTypesQuery);
            }

            return query;
        }
    }
}