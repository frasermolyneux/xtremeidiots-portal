using System.Linq;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Servers.Models;

namespace XI.Portal.Servers.Extensions
{
    public static class RconMonitorsQueryExtensions
    {
        public static IQueryable<RconMonitors> ApplyFilter(this IQueryable<RconMonitors> rconMonitors, RconMonitorFilterModel filterModel)
        {
            rconMonitors = rconMonitors.Include(bfm => bfm.GameServerServer).AsQueryable();

            if (filterModel.GameTypes != null)
                rconMonitors = rconMonitors.Where(rm => filterModel.GameTypes.Contains(rm.GameServerServer.GameType)).AsQueryable();

            rconMonitors = rconMonitors.Skip(filterModel.SkipEntries).AsQueryable();

            switch (filterModel.Order)
            {
                case RconMonitorFilterModel.OrderBy.BannerServerListPosition:
                    rconMonitors = rconMonitors.OrderBy(rm => rm.GameServerServer.BannerServerListPosition).AsQueryable();
                    break;
                case RconMonitorFilterModel.OrderBy.GameType:
                    rconMonitors = rconMonitors.OrderBy(rm => rm.GameServerServer.GameType).AsQueryable();
                    break;
            }

            if (filterModel.TakeEntries != 0) rconMonitors = rconMonitors.Take(filterModel.TakeEntries).AsQueryable();

            return rconMonitors;
        }
    }
}