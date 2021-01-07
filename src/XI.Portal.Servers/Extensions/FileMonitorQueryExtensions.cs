using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Servers.Models;

namespace XI.Portal.Servers.Extensions
{
    public static class FileMonitorQueryExtensions
    {
        public static IQueryable<FileMonitors> ApplyFilter(this IQueryable<FileMonitors> fileMonitors, FileMonitorFilterModel filterModel)
        {
            fileMonitors = fileMonitors.Include(bfm => bfm.GameServerServer).AsQueryable();

            if (filterModel.GameTypes != null)
                fileMonitors = fileMonitors.Where(fm => filterModel.GameTypes.Contains(fm.GameServerServer.GameType)).AsQueryable();

            if (filterModel.ServerId != Guid.Empty)
                fileMonitors = fileMonitors.Where(fm => fm.GameServerServerId == filterModel.ServerId).AsQueryable();

            fileMonitors = fileMonitors.Skip(filterModel.SkipEntries).AsQueryable();

            switch (filterModel.Order)
            {
                case FileMonitorFilterModel.OrderBy.BannerServerListPosition:
                    fileMonitors = fileMonitors.OrderBy(fm => fm.GameServerServer.BannerServerListPosition).AsQueryable();
                    break;
                case FileMonitorFilterModel.OrderBy.GameType:
                    fileMonitors = fileMonitors.OrderBy(fm => fm.GameServerServer.GameType).AsQueryable();
                    break;
            }

            if (filterModel.TakeEntries != 0) fileMonitors = fileMonitors.Take(filterModel.TakeEntries).AsQueryable();

            return fileMonitors;
        }
    }
}