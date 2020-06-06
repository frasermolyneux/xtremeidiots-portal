using System;
using System.Threading.Tasks;
using XI.Portal.Servers.Integrations.Interfaces;

namespace XI.Portal.Servers.Integrations
{
    public class DucCommand : IChatCommand
    {
        public string CommandText => "!duc";

        public Task ProcessMessage(Guid serverId, string name, string guid, string message)
        {
            return Task.CompletedTask;
        }
    }
}