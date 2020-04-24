using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.Models;
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
                .ToListAsync();

            var adminActionsEntryViewModels = adminActions.Select(aa => new AdminActionDto
            {
                AdminActionId = aa.AdminActionId,
                PlayerId = aa.PlayerPlayer.PlayerId,
                GameType = aa.PlayerPlayer.GameType,
                Username = aa.PlayerPlayer.Username,
                Guid = aa.PlayerPlayer.Guid,
                Type = aa.Type,
                Text = aa.Text,
                Expires = aa.Expires,
                ForumTopicId = aa.ForumTopicId,
                Created = aa.Created,
                AdminId = aa.Admin.XtremeIdiotsId,
                AdminName = aa.Admin.UserName
            }).ToList();

            return adminActionsEntryViewModels;
        }

        public async Task Create(AdminActionDto adminAction)
        {
            var player = await _legacyContext.Player2.SingleAsync(p => p.PlayerId == adminAction.PlayerId);
            var admin = await _legacyContext.AspNetUsers.SingleAsync(a => a.XtremeIdiotsId == adminAction.AdminId);

            var model = new AdminActions
            {
                PlayerPlayer = player,
                Admin = admin,
                Type = adminAction.Type,
                Text = adminAction.Text,
                Created = adminAction.Created,
                Expires = adminAction.Expires
            };

            _legacyContext.AdminActions.Add(model);
            await _legacyContext.SaveChangesAsync();
        }

        public async Task<AdminActionDto> GetAdminAction(Guid id)
        {
            var adminAction = await _legacyContext.AdminActions
                .Include(aa => aa.PlayerPlayer)
                .Include(aa => aa.Admin)
                .SingleAsync(aa => aa.AdminActionId == id);

            return new AdminActionDto
            {
                AdminActionId = adminAction.AdminActionId,
                PlayerId = adminAction.PlayerPlayer.PlayerId,
                GameType = adminAction.PlayerPlayer.GameType,
                Username = adminAction.PlayerPlayer.Username,
                Guid = adminAction.PlayerPlayer.Guid,
                Type = adminAction.Type,
                Text = adminAction.Text,
                Expires = adminAction.Expires,
                ForumTopicId = adminAction.ForumTopicId,
                Created = adminAction.Created,
                AdminId = adminAction.Admin.XtremeIdiotsId,
                AdminName = adminAction.Admin.UserName
            };
        }

        public async Task Delete(Guid id)
        {
            var adminAction = await _legacyContext.AdminActions
                .SingleAsync(aa => aa.AdminActionId == id);

            _legacyContext.Remove(adminAction);
            await _legacyContext.SaveChangesAsync();
        }

        public async Task UpdateAdminAction(AdminActionDto model)
        {
            var adminAction = await _legacyContext.AdminActions.SingleAsync(aa => aa.AdminActionId == model.AdminActionId);

            adminAction.Text = model.Text;
            adminAction.Expires = model.Expires;

            if (adminAction.AdminId != model.AdminId)
            {
                adminAction.Admin = await _legacyContext.AspNetUsers.SingleAsync(u => u.XtremeIdiotsId == model.AdminId);
            }

            await _legacyContext.SaveChangesAsync();
        }
    }
}