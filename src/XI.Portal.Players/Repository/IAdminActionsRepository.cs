using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Players.Models;

namespace XI.Portal.Players.Repository
{
    public interface IAdminActionsRepository
    {
        Task<int> GetAdminActionsListCount(AdminActionsFilterModel filterModel);
        Task<List<AdminActionListEntryViewModel>> GetAdminActionsList(AdminActionsFilterModel filterModel);
    }
}