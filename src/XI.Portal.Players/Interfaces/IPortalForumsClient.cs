using System.Threading.Tasks;
using XI.Portal.Players.Dto;

namespace XI.Portal.Players.Interfaces
{
    public interface IPortalForumsClient
    {
        Task<int> CreateTopicForAdminAction(AdminActionDto model);
        Task UpdateTopicForAdminAction(AdminActionDto model);
    }
}