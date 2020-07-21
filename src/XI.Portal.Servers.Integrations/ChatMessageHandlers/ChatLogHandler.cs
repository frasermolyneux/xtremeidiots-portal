using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using XI.CommonTypes;
using XI.Portal.Players.Interfaces;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Integrations.Interfaces;
using XI.Portal.Servers.Interfaces;

namespace XI.Portal.Servers.Integrations.ChatMessageHandlers
{
    public class ChatLogHandler : IChatMessageHandler
    {
        private readonly IGameServersRepository _gameServersRepository;
        private readonly ILogger<ChatLogHandler> _logger;
        private readonly IPlayersRepository _playersRepository;
        private readonly IChatLogsRepository _chatLogsRepository;

        public ChatLogHandler(ILogger<ChatLogHandler> logger,
            IGameServersRepository gameServersRepository,
            IPlayersRepository playersRepository,
            IChatLogsRepository chatLogsRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
            _playersRepository = playersRepository ?? throw new ArgumentNullException(nameof(playersRepository));
            _chatLogsRepository = chatLogsRepository ?? throw new ArgumentNullException(nameof(chatLogsRepository));
        }

        public async Task HandleChatMessage(Guid serverId, string name, string guid, string message, ChatType chatType)
        {
            var server = await _gameServersRepository.GetGameServer(serverId);

            var databasePlayer = await _playersRepository.GetPlayer(server.GameType, guid);
            if (databasePlayer == null)
            {
                _logger.LogWarning("Could not handle chat message {message} for {name} as player is null for {guid} on server {serverId}", message, name, guid, serverId);
                return;
            }

            _logger.Log(LogLevel.Information, $"[{server.Title}] {name} :: {message}");

            await _chatLogsRepository.CreateChatLog(new ChatLogDto()
            {
                PlayerId = databasePlayer.PlayerId,
                ServerId = serverId,
                Username = name,
                ChatType = chatType.ToString(),
                Message = message
            });
        }
    }
}