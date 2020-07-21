using System;
using System.Threading.Tasks;

namespace XI.Portal.Servers.Integrations.Interfaces
{
    public interface IChatMessageHandler
    {
        Task HandleChatMessage(Guid serverId, string name, string guid, string message);
    }
}