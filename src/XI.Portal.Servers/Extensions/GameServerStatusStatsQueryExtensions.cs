using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using System;
using XI.Portal.Servers.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Portal.Servers.Extensions
{
    internal static class GameServerStatusStatsQueryExtensions
    {
        internal static TableQuery<GameServerStatusStatsEntity> ApplyFilter(this TableQuery<GameServerStatusStatsEntity> query, GameServerStatusStatsFilterModel filterModel)
        {
            var serverIdFilter = TableQuery.GenerateFilterCondition(nameof(GameServerStatusStatsEntity.PartitionKey), QueryComparisons.Equal, filterModel.ServerId.ToString());
            var gameTypeFilter = TableQuery.GenerateFilterCondition(nameof(GameServerStatusStatsEntity.GameType), QueryComparisons.Equal, filterModel.GameType.ToString());

            var dateTimeFilter = string.Empty;
            if (filterModel.Cutoff != null)
                dateTimeFilter = TableQuery.GenerateFilterConditionForDate(nameof(GameServerStatusStatsEntity.Timestamp), QueryComparisons.GreaterThanOrEqual, (DateTime)filterModel.Cutoff);

            if (filterModel.ServerId != Guid.Empty && filterModel.Cutoff != null)
                query = query.Where(TableQuery.CombineFilters(serverIdFilter, TableOperators.And, dateTimeFilter)).AsTableQuery();
            else if (filterModel.GameType != GameType.Unknown && filterModel.Cutoff != null)
                query = query.Where(TableQuery.CombineFilters(gameTypeFilter, TableOperators.And, dateTimeFilter)).AsTableQuery();
            else if (filterModel.ServerId != Guid.Empty)
                query = query.Where(serverIdFilter);
            else if (filterModel.GameType != GameType.Unknown) query = query.Where(gameTypeFilter);

            return query;
        }
    }
}