using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.CommonTypes;
using XI.Portal.Players.Configuration;
using XI.Portal.Players.Extensions;
using XI.Portal.Players.Interfaces;
using XI.Portal.Players.Models;

namespace XI.Portal.Players.Repository
{
    public class AdminActionsRepository : IAdminActionsRepository
    {
        private readonly LegacyPortalContext _legacyContext;
        private readonly IAdminActionsRepositoryOptions _options;

        public AdminActionsRepository(IAdminActionsRepositoryOptions options, LegacyPortalContext legacyContext)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
        }

        public async Task<int> GetAdminActionsListCount(AdminActionsFilterModel filterModel)
        {
            if (filterModel == null) filterModel = new AdminActionsFilterModel();

            return await filterModel.ApplyFilter(_legacyContext).CountAsync();
        }

        public async Task<List<AdminActionListEntryViewModel>> GetAdminActionsList(AdminActionsFilterModel filterModel)
        {
            if (filterModel == null) filterModel = new AdminActionsFilterModel();

            var adminActions = await filterModel.ApplyFilter(_legacyContext).Include(aa => aa.PlayerPlayer).ToListAsync();

            var adminActionsEntryViewModels = adminActions.Select(aa => new AdminActionListEntryViewModel
            {
                PlayerId = aa.PlayerPlayer.PlayerId,
                Username = aa.PlayerPlayer.Username,
                Guid = aa.PlayerPlayer.Guid,
                Type = aa.Type.ToString(),
                Expires = aa.Type == AdminActionType.Ban ? "Never" : aa.Expires.ToString(),
                Created = aa.Created
            }).ToList();

            return adminActionsEntryViewModels;
        }
    }
}