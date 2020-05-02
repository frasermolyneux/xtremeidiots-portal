using System.Threading.Tasks;
using XI.Portal.Players.Dto;

namespace XI.Portal.Players.Interfaces
{
    public interface IPlayersForumsClient
    {
        Task<int> CreateTopicForAdminAction(AdminActionDto model);
        Task UpdateTopicForAdminAction(AdminActionDto model);
    }
}