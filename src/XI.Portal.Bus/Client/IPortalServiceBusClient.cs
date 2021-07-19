using System.Threading.Tasks;
using XI.Portal.Bus.Models;

namespace XI.Portal.Bus.Client
{
    public interface IPortalServiceBusClient
    {
        Task PostMapVote(MapVote model);
        Task PostPlayerAuth(PlayerAuth playerAuth);
        Task PostChatMessageEvent(ChatMessage chatMessage);
    }
}