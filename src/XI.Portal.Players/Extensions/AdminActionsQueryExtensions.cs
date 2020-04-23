using System;
using System.Linq;
using XI.CommonTypes;
using XI.Portal.Data.Legacy.CommonTypes;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Players.Models;

namespace XI.Portal.Players.Extensions
{
    public static class AdminActionsQueryExtensions
    {
        public static IQueryable<AdminActions> ApplyFilter(this IQueryable<AdminActions> adminActions, AdminActionsFilterModel filterModel)
        {
            if (filterModel.GameType != GameType.Unknown) adminActions = adminActions.Where(aa => aa.PlayerPlayer.GameType == filterModel.GameType).AsQueryable();

            if (filterModel.PlayerId != Guid.Empty) adminActions = adminActions.Where(aa => aa.PlayerPlayerId == filterModel.PlayerId).AsQueryable();

            switch (filterModel.Filter)
            {
                case AdminActionsFilterModel.FilterType.ActiveBans:
                    adminActions = adminActions.Where(aa => aa.Type == AdminActionType.Ban && aa.Expires == null
                                                            || aa.Type == AdminActionType.TempBan && aa.Expires > DateTime.UtcNow)
                        .AsQueryable();
                    break;
                case AdminActionsFilterModel.FilterType.UnclaimedBans:
                    adminActions = adminActions.Where(aa => aa.Type == AdminActionType.Ban && aa.Expires == null
                                                                                           && aa.Admin == null)
                        .AsQueryable();
                    break;
            }

            switch (filterModel.Order)
            {
                case AdminActionsFilterModel.OrderBy.CreatedAsc:
                    adminActions = adminActions.OrderBy(aa => aa.Created).AsQueryable();
                    break;
                case AdminActionsFilterModel.OrderBy.CreatedDesc:
                    adminActions = adminActions.OrderByDescending(aa => aa.Created).AsQueryable();
                    break;
            }

            adminActions = adminActions.Skip(filterModel.SkipEntries).AsQueryable();

            if (filterModel.TakeEntries != 0) adminActions = adminActions.Take(filterModel.TakeEntries).AsQueryable();

            return adminActions;
        }
    }
}