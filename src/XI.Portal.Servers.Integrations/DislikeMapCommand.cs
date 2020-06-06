using System;
using System.Threading.Tasks;
using XI.Portal.Servers.Integrations.Interfaces;

namespace XI.Portal.Servers.Integrations
{
    public class DislikeMapCommand : IChatCommand
    {
        public string CommandText => "!dislike";

        public Task ProcessMessage(Guid serverId, string name, string guid, string message)
        {
            return Task.CompletedTask;
        }
    }
}