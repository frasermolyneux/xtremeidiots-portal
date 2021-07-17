using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using XI.CommonTypes;
using XI.Portal.Maps.Models;

namespace XI.Portal.Maps.Extensions
{
    public static class MapsQueryExtensions
    {
        public static IQueryable<Data.Legacy.Models.Maps> ApplyFilter(this IQueryable<Data.Legacy.Models.Maps> maps, MapsFilterModel filterModel)
        {
            maps = maps.Include(m => m.MapFiles).AsQueryable();

            if (filterModel.GameType != GameType.Unknown) maps = maps.Where(m => m.GameType == filterModel.GameType).AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterModel.FilterString)) maps = maps.Where(m => m.MapName.Contains(filterModel.FilterString)).AsQueryable();

            if (filterModel.MapNames != null) maps = maps.Where(m => filterModel.MapNames.Contains(m.MapName)).AsQueryable();

            switch (filterModel.Order)
            {
                case MapsFilterModel.OrderBy.MapNameAsc:
                    maps = maps.OrderBy(m => m.MapName).AsQueryable();
                    break;
                case MapsFilterModel.OrderBy.MapNameDesc:
                    maps = maps.OrderByDescending(m => m.MapName).AsQueryable();
                    break;
                case MapsFilterModel.OrderBy.GameTypeAsc:
                    maps = maps.OrderBy(m => m.GameType).AsQueryable();
                    break;
                case MapsFilterModel.OrderBy.GameTypeDesc:
                    maps = maps.OrderByDescending(m => m.GameType).AsQueryable();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            maps = maps.Skip(filterModel.SkipEntries).AsQueryable();

            if (filterModel.TakeEntries != 0) maps = maps.Take(filterModel.TakeEntries).AsQueryable();

            return maps;
        }
    }
}