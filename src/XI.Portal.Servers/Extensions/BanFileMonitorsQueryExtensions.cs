using System.Linq;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Servers.Models;

namespace XI.Portal.Servers.Extensions
{
    public static class BanFileMonitorsQueryExtensions
    {
        public static IQueryable<BanFileMonitors> ApplyFilter(this IQueryable<BanFileMonitors> banFileMonitors, BanFileMonitorFilterModel filterModel)
        {
            banFileMonitors = banFileMonitors.Include(bfm => bfm.GameServerServer).AsQueryable();

            if (filterModel.GameTypes != null)
                banFileMonitors = banFileMonitors.Where(bfm => filterModel.GameTypes.Contains(bfm.GameServerServer.GameType)).AsQueryable();

            banFileMonitors = banFileMonitors.Skip(filterModel.SkipEntries).AsQueryable();

            switch (filterModel.Order)
            {
                case BanFileMonitorFilterModel.OrderBy.BannerServerListPosition:
                    banFileMonitors = banFileMonitors.OrderBy(bfm => bfm.GameServerServer.BannerServerListPosition).AsQueryable();
                    break;
                case BanFileMonitorFilterModel.OrderBy.GameType:
                    banFileMonitors = banFileMonitors.OrderBy(bfm => bfm.GameServerServer.GameType).AsQueryable();
                    break;
            }

            if (filterModel.TakeEntries != 0) banFileMonitors = banFileMonitors.Take(filterModel.TakeEntries).AsQueryable();

            return banFileMonitors;
        }
    }
}