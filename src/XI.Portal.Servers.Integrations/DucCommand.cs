using System;
using System.Threading.Tasks;
using XI.Portal.Servers.Integrations.Interfaces;
using XI.Portal.Servers.Interfaces;
using XI.Servers.Interfaces;

namespace XI.Portal.Servers.Integrations
{
    public class DucCommand : IChatCommand
    {
        private readonly IGameServersRepository _gameServersRepository;
        private readonly IRconClientFactory _rconClientFactory;

        public DucCommand(
            IGameServersRepository gameServersRepository, 
            IRconClientFactory rconClientFactory)
        {
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
            _rconClientFactory = rconClientFactory ?? throw new ArgumentNullException(nameof(rconClientFactory));
        }

        public string[] CommandAliases { get; } = {"!duc"};

        public async Task ProcessMessage(Guid serverId, string name, string guid, string message)
        {
            if (name != "Sitting-Duc>XI<")
                return;

            var server = await _gameServersRepository.GetGameServer(serverId);
            var rconClient = _rconClientFactory.CreateInstance(server.GameType, server.ServerId, server.Hostname, server.QueryPort, server.RconPassword);
            await rconClient.Say($"^5Sitting-Duc ^2is the greatest!");
        }
    }
}