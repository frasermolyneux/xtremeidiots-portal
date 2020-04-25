using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Players.Dto;
using XI.Portal.Players.Models;

namespace XI.Portal.Players.Interfaces
{
    public interface IAdminActionsRepository
    {
        Task<int> GetAdminActionsCount(AdminActionsFilterModel filterModel);
        Task<List<AdminActionDto>> GetAdminActions(AdminActionsFilterModel filterModel);
        Task<AdminActionDto> GetAdminAction(Guid adminActionId);
        Task CreateAdminAction(AdminActionDto adminActionDto);
        Task UpdateAdminAction(AdminActionDto adminActionDto);
        Task DeleteAdminAction(Guid adminActionId);
    }
}