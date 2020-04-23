using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy;
using XI.Portal.Players.Dto;
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

            return await _legacyContext.AdminActions.ApplyFilter(filterModel).CountAsync();
        }

        public async Task<List<AdminActionDto>> GetAdminActions(AdminActionsFilterModel filterModel)
        {
            if (filterModel == null) filterModel = new AdminActionsFilterModel();

            var adminActions = await _legacyContext.AdminActions
                .ApplyFilter(filterModel)
                .Include(aa => aa.PlayerPlayer)
                .Include(aa => aa.Admin)
                .ToListAsync();

            var adminActionsEntryViewModels = adminActions.Select(aa => new AdminActionDto
            {
                AdminActionId = aa.AdminActionId,
                PlayerId = aa.PlayerPlayer.PlayerId,
                Username = aa.PlayerPlayer.Username,
                Guid = aa.PlayerPlayer.Guid,
                Type = aa.Type,
                Text = aa.Text,
                Expires = aa.Expires,
                ForumTopicId = aa.ForumTopicId,
                Created = aa.Created,
                AdminId = aa.AdminId,
                AdminName = aa.Admin.XtremeIdiotsId
            }).ToList();

            return adminActionsEntryViewModels;
        }
    }
}