using System;
using System.Threading.Tasks;
using XI.Portal.Servers.Interfaces;
using XI.Servers.Interfaces;

namespace XI.Portal.Servers.Integrations.ChatMessageHandlers
{
    public class DucCommandHandler : ChatCommandHandlerBase
    {
        private readonly IGameServersRepository _gameServersRepository;
        private readonly IRconClientFactory _rconClientFactory;

        public DucCommandHandler(
            IGameServersRepository gameServersRepository,
            IRconClientFactory rconClientFactory) : base(new[] {"!duc"})
        {
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
            _rconClientFactory = rconClientFactory ?? throw new ArgumentNullException(nameof(rconClientFactory));
        }

        public override async Task HandleChatMessage(Guid serverId, string name, string guid, string message)
        {
            if (!IsMatchingCommand(message))
                return;

            if (name != "Sitting-Duc>XI<")
                return;

            var server = await _gameServersRepository.GetGameServer(serverId);
            var rconClient = _rconClientFactory.CreateInstance(server.GameType, server.ServerId, server.Hostname, server.QueryPort, server.RconPassword);
            await rconClient.Say("^5Sitting-Duc ^2is the greatest!");
        }
    }
}