using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using XI.Portal.Players.Interfaces;
using XI.Portal.Servers.Integrations.Interfaces;
using XI.Portal.Servers.Interfaces;

namespace XI.Portal.Servers.Integrations.ChatMessageHandlers
{
    public class ChatLogHandler : IChatMessageHandler
    {
        private readonly IGameServersRepository _gameServersRepository;
        private readonly ILogger<ChatLogHandler> _logger;
        private readonly IPlayersRepository _playersRepository;

        public ChatLogHandler(ILogger<ChatLogHandler> logger,
            IGameServersRepository gameServersRepository,
            IPlayersRepository playersRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
            _playersRepository = playersRepository;
        }

        public async Task HandleChatMessage(Guid serverId, string name, string guid, string message)
        {
            var server = await _gameServersRepository.GetGameServer(serverId);

            var databasePlayer = await _playersRepository.GetPlayer(server.GameType, guid);
            if (databasePlayer == null)
            {
                _logger.LogWarning("Could not handle chat message {message} for {name} as player is null for {guid} on server {serverId}", message, name, guid, serverId);
                return;
            }

            _logger.Log(LogLevel.Information, $"[{server.Title}] {name} :: {message}");
        }
    }
}