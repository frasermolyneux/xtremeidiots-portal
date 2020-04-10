using System;
using System.Linq;
using XI.CommonTypes;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Players.Models;

namespace XI.Portal.Players.Extensions
{
    public static class PlayerFilterModelExtensions
    {
        public static IQueryable<Player2> ApplyFilter(this PlayersFilterModel filterModel, LegacyPortalContext context)
        {
            var players = context.Player2.AsQueryable();

            if (filterModel.GameType != GameType.Unknown) players = players.Where(p => p.GameType == filterModel.GameType).AsQueryable();

            if (filterModel.Filter != PlayersFilterModel.FilterType.None && !string.IsNullOrWhiteSpace(filterModel.FilterString))
                switch (filterModel.Filter)
                {
                    case PlayersFilterModel.FilterType.UsernameAndGuid:
                        players = players.Where(p => p.Username.Contains(filterModel.FilterString) ||
                                                     p.Guid.Contains(filterModel.FilterString) ||
                                                     p.PlayerAlias.Any(a => a.Name.Contains(filterModel.FilterString)))
                            .AsQueryable();
                        break;
                    case PlayersFilterModel.FilterType.IpAddress:
                        players = players.Where(p => p.IpAddress.Contains(filterModel.FilterString) ||
                                                     p.PlayerIpAddresses.Any(ip => ip.Address.Contains(filterModel.FilterString)))
                            .AsQueryable();
                        break;
                }
            else if (filterModel.Filter == PlayersFilterModel.FilterType.IpAddress) players = players.Where(p => p.IpAddress != "" && p.IpAddress != null).AsQueryable();

            switch (filterModel.Order)
            {
                case PlayersFilterModel.OrderBy.UsernameAsc:
                    players = players.OrderBy(p => p.Username).AsQueryable();
                    break;
                case PlayersFilterModel.OrderBy.UsernameDesc:
                    players = players.OrderByDescending(p => p.Username).AsQueryable();
                    break;
                case PlayersFilterModel.OrderBy.FirstSeenAsc:
                    players = players.OrderBy(p => p.FirstSeen).AsQueryable();
                    break;
                case PlayersFilterModel.OrderBy.FirstSeenDesc:
                    players = players.OrderByDescending(p => p.FirstSeen).AsQueryable();
                    break;
                case PlayersFilterModel.OrderBy.LastSeenAsc:
                    players = players.OrderBy(p => p.LastSeen).AsQueryable();
                    break;
                case PlayersFilterModel.OrderBy.LastSeenDesc:
                    players = players.OrderByDescending(p => p.LastSeen).AsQueryable();
                    break;
                case PlayersFilterModel.OrderBy.GameTypeAsc:
                    players = players.OrderBy(p => p.GameType).AsQueryable();
                    break;
                case PlayersFilterModel.OrderBy.GameTypeDesc:
                    players = players.OrderByDescending(p => p.GameType).AsQueryable();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            players = players.Skip(filterModel.SkipEntries).AsQueryable();

            if (filterModel.TakeEntries != 0) players = players.Take(filterModel.TakeEntries).AsQueryable();

            return players;
        }
    }
}