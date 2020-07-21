using System;
using System.Linq;
using System.Threading.Tasks;
using XI.Portal.Servers.Integrations.Interfaces;

namespace XI.Portal.Servers.Integrations.ChatMessageHandlers
{
    public class ChatCommandHandlerBase : IChatMessageHandler
    {
        public ChatCommandHandlerBase(string[] commandAliases)
        {
            CommandAliases = commandAliases;
        }

        public virtual string[] CommandAliases { get; }

        public bool IsMatchingCommand(string message)
        {
            var messageParts = message.ToLower().Split(' ');

            if (!messageParts.Any()) 
                return false;

            var commandPart = messageParts.First();

            return CommandAliases.Contains(commandPart);
        }

        public virtual Task HandleChatMessage(Guid serverId, string name, string guid, string message)
        {
            return Task.CompletedTask;
        }
    }
}