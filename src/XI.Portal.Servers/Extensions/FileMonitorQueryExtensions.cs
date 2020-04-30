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
                fileMonitors = fileMonitors.Where(bfm => filterModel.GameTypes.Contains(bfm.GameServerServer.GameType)).AsQueryable();

            fileMonitors = fileMonitors.Skip(filterModel.SkipEntries).AsQueryable();

            switch (filterModel.Order)
            {
                case FileMonitorFilterModel.OrderBy.BannerServerListPosition:
                    fileMonitors = fileMonitors.OrderBy(bfm => bfm.GameServerServer.BannerServerListPosition).AsQueryable();
                    break;
                case FileMonitorFilterModel.OrderBy.GameType:
                    fileMonitors = fileMonitors.OrderBy(bfm => bfm.GameServerServer.GameType).AsQueryable();
                    break;
            }

            if (filterModel.TakeEntries != 0) fileMonitors = fileMonitors.Take(filterModel.TakeEntries).AsQueryable();

            return fileMonitors;
        }
    }
}