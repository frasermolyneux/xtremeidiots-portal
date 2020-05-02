using System.Linq;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Servers.Models;

namespace XI.Portal.Servers.Extensions
{
    public static class GameServersQueryExtensions
    {
        public static IQueryable<GameServers> ApplyFilter(this IQueryable<GameServers> gameServers, GameServerFilterModel filterModel)
        {
            if (filterModel.GameTypes != null && filterModel.ServerIds != null)
                gameServers = gameServers.Where(s => filterModel.GameTypes.Contains(s.GameType) || filterModel.ServerIds.Contains(s.ServerId)).AsQueryable();
            else if (filterModel.GameTypes != null)
                gameServers = gameServers.Where(s => filterModel.GameTypes.Contains(s.GameType)).AsQueryable();
            else if (filterModel.ServerIds != null) gameServers = gameServers.Where(s => filterModel.ServerIds.Contains(s.ServerId)).AsQueryable();

            switch (filterModel.Filter)
            {
                case GameServerFilterModel.FilterBy.ShowOnPortalServerList:
                    gameServers = gameServers.Where(s => s.ShowOnPortalServerList);
                    break;
            }

            gameServers = gameServers.Skip(filterModel.SkipEntries).AsQueryable();

            switch (filterModel.Order)
            {
                case GameServerFilterModel.OrderBy.BannerServerListPosition:
                    gameServers = gameServers.OrderBy(s => s.BannerServerListPosition).AsQueryable();
                    break;
                case GameServerFilterModel.OrderBy.GameType:
                    gameServers = gameServers.OrderBy(s => s.GameType).AsQueryable();
                    break;
            }

            if (filterModel.TakeEntries != 0) gameServers = gameServers.Take(filterModel.TakeEntries).AsQueryable();

            return gameServers;
        }
    }
}