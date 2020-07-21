using System;
using System.Threading.Tasks;
using XI.CommonTypes;

namespace XI.Portal.Servers.Integrations.Interfaces
{
    public interface IChatMessageHandler
    {
        Task HandleChatMessage(Guid serverId, string name, string guid, string message, ChatType chatType);
    }
}