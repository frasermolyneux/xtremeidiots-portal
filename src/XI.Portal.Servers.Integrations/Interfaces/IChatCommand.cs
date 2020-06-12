using System;
using System.Threading.Tasks;

namespace XI.Portal.Servers.Integrations.Interfaces
{
    public interface IChatCommand
    {
        string[] CommandAliases { get; }
        Task ProcessMessage(Guid serverId, string name, string guid, string message);
    }
}