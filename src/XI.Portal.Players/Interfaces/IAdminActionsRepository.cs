using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Players.Dto;
using XI.Portal.Players.Models;

namespace XI.Portal.Players.Interfaces
{
    public interface IAdminActionsRepository
    {
        Task<int> GetAdminActionsListCount(AdminActionsFilterModel filterModel);
        Task<List<AdminActionDto>> GetAdminActions(AdminActionsFilterModel filterModel);
    }
}