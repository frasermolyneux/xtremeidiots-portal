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

        public AdminActionsRepository(LegacyPortalContext legacyContext)
        {
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
        }

        public async Task<int> GetAdminActionsCount(AdminActionsFilterModel filterModel)
        {
            if (filterModel == null) throw new ArgumentNullException(nameof(filterModel));

            return await _legacyContext.AdminActions.ApplyFilter(filterModel).CountAsync();
        }

        public async Task<List<AdminActionDto>> GetAdminActions(AdminActionsFilterModel filterModel)
        {
            if (filterModel == null) throw new ArgumentNullException(nameof(filterModel));

            var adminActions = await _legacyContext.AdminActions
                .ApplyFilter(filterModel)
                .ToListAsync();

            var models = adminActions.Select(aa => aa.ToDto()).ToList();

            return models;
        }

        public async Task<AdminActionDto> GetAdminAction(Guid adminActionId)
        {
            var adminAction = await _legacyContext.AdminActions
                .Include(aa => aa.PlayerPlayer)
                .Include(aa => aa.Admin)
                .SingleOrDefaultAsync(aa => aa.AdminActionId == adminActionId);

            return adminAction?.ToDto();
        }

        public async Task CreateAdminAction(AdminActionDto adminActionDto)
        {
            if (adminActionDto == null) throw new ArgumentNullException(nameof(adminActionDto));

            var player = await _legacyContext.Player2.SingleOrDefaultAsync(p => p.PlayerId == adminActionDto.PlayerId);

            if (player == null)
                throw new NullReferenceException(nameof(player));

            var admin = await _legacyContext.AspNetUsers.SingleOrDefaultAsync(u => u.XtremeIdiotsId == adminActionDto.AdminId);

            if (admin == null)
                throw new NullReferenceException(nameof(admin));

            var adminAction = new AdminActions
            {
                PlayerPlayer = player,
                Admin = admin,
                Type = adminActionDto.Type,
                Text = adminActionDto.Text,
                Created = adminActionDto.Created,
                Expires = adminActionDto.Expires,
                ForumTopicId = adminActionDto.ForumTopicId
            };

            _legacyContext.AdminActions.Add(adminAction);
            await _legacyContext.SaveChangesAsync();
        }

        public async Task UpdateAdminAction(AdminActionDto adminActionDto)
        {
            if (adminActionDto == null) throw new ArgumentNullException(nameof(adminActionDto));

            var adminAction = await _legacyContext.AdminActions.SingleOrDefaultAsync(aa => aa.AdminActionId == adminActionDto.AdminActionId);

            if (adminAction == null)
                throw new NullReferenceException(nameof(adminAction));

            adminAction.Text = adminActionDto.Text;
            adminAction.Expires = adminActionDto.Expires;

            if (adminAction.AdminId != adminActionDto.AdminId)
            {
                if (string.IsNullOrWhiteSpace(adminActionDto.AdminId))
                    adminAction.Admin = null;
                else
                {
                    var admin = await _legacyContext.AspNetUsers.SingleOrDefaultAsync(u => u.XtremeIdiotsId == adminActionDto.AdminId);

                    if (admin == null)
                        throw new NullReferenceException(nameof(admin));

                    adminAction.Admin = admin;
                }
            }

            if (adminActionDto.ForumTopicId != 0)
                adminAction.ForumTopicId = adminActionDto.ForumTopicId;

            await _legacyContext.SaveChangesAsync();
        }

        public async Task DeleteAdminAction(Guid adminActionId)
        {
            var adminAction = await _legacyContext.AdminActions
                .SingleOrDefaultAsync(aa => aa.AdminActionId == adminActionId);

            if (adminAction == null)
                throw new NullReferenceException(nameof(adminAction));

            _legacyContext.Remove(adminAction);
            await _legacyContext.SaveChangesAsync();
        }
    }
}