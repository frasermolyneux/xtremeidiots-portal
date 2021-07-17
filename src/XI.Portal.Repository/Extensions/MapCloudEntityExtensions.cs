using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Cosmos.Table;
using XI.CommonTypes;
using XI.Portal.Repository.CloudEntities;
using XI.Portal.Repository.Dtos;
using XI.Portal.Repository.Models;

namespace XI.Portal.Repository.Extensions
{
    internal static class MapCloudEntityExtensions
    {
        internal static MapDto ToDto(this MapCloudEntity mapCloudEntity)
        {
            return new MapDto
            {
                GameType = Enum.Parse<GameType>(mapCloudEntity.PartitionKey),
                MapName = mapCloudEntity.RowKey,
                PositiveVotes = mapCloudEntity.PositiveVotes,
                NegativeVotes = mapCloudEntity.NegativeVotes,
                TotalVotes = mapCloudEntity.TotalVotes,
                PositivePercentage = mapCloudEntity.PositivePercentage,
                NegativePercentage = mapCloudEntity.NegativePercentage,
                MapFiles = mapCloudEntity.MapFiles ?? new List<MapFileDto>()
            };
        }

        internal static TableQuery<MapCloudEntity> ApplyQueryOptions(this TableQuery<MapCloudEntity> query, MapsQueryOptions queryOptions)
        {
            if (queryOptions.GameType != GameType.Unknown)
                query = query.Where(TableQuery.GenerateFilterCondition(nameof(MapCloudEntity.PartitionKey), QueryComparisons.Equal, queryOptions.GameType.ToString()));

            return query;
        }

        internal static List<MapCloudEntity> ApplyQueryOptions(this List<MapCloudEntity> entities, MapsQueryOptions queryOptions)
        {
            var entitiesAsQueryable = entities.AsQueryable();

            switch (queryOptions.Order)
            {
                case MapsQueryOptions.OrderBy.MapNameAsc:
                    entitiesAsQueryable = entitiesAsQueryable.OrderBy(e => e.RowKey);
                    break;
                case MapsQueryOptions.OrderBy.MapNameDesc:
                    entitiesAsQueryable = entitiesAsQueryable.OrderByDescending(e => e.RowKey);
                    break;
                case MapsQueryOptions.OrderBy.LikeDislikeAsc:
                    entitiesAsQueryable = entitiesAsQueryable.OrderByDescending(e => e.PositiveVotes);
                    break;
                case MapsQueryOptions.OrderBy.LikeDislikeDesc:
                    entitiesAsQueryable = entitiesAsQueryable.OrderByDescending(e => e.NegativeVotes);
                    break;
                case MapsQueryOptions.OrderBy.GameTypeAsc:
                    entitiesAsQueryable = entitiesAsQueryable.OrderBy(e => e.PartitionKey);
                    break;
                case MapsQueryOptions.OrderBy.GameTypeDesc:
                    entitiesAsQueryable = entitiesAsQueryable.OrderByDescending(e => e.PartitionKey);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(queryOptions.FilterString))
                entitiesAsQueryable = entitiesAsQueryable.Where(e => e.RowKey.Contains(queryOptions.FilterString));

            if (queryOptions.SkipEntries != 0)
                entitiesAsQueryable = entitiesAsQueryable.Skip(queryOptions.SkipEntries);

            if (queryOptions.TakeEntries != 0)
                entitiesAsQueryable = entitiesAsQueryable.Take(queryOptions.TakeEntries);

            if (queryOptions.MapNames != null && queryOptions.MapNames.Any())
                entitiesAsQueryable = entitiesAsQueryable.Where(e => queryOptions.MapNames.Contains(e.RowKey));

            return entitiesAsQueryable.ToList();
        }
    }
}