using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Web.Models;

namespace XI.Portal.Web.Repository
{
    public interface IUsersRepository
    {
        Task<List<UserListEntryViewModel>> GetUsers();
        Task<List<IPortalClaimDto>> GetUserClaims(string userId);
        Task UpdatePortalClaim(string userId, PortalClaimDto portalClaimDto);
        Task<UserListEntryViewModel> GetUser(string userId);
        Task RemoveUserClaim(string userId, string claimId);
    }
}