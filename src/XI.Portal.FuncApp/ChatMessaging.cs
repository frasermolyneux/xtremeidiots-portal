using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using XI.Portal.Bus.Models;
using XI.Portal.Players.Interfaces;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Interfaces;

namespace XI.Portal.FuncApp
{
    internal class ChatMessaging
    {
        private readonly IChatLogsRepository _chatLogsRepository;
        private readonly IGameServersRepository _gameServersRepository;
        private readonly IPlayersRepository _playersRepository;

        public ChatMessaging(
            IGameServersRepository gameServersRepository,
            IPlayersRepository playersRepository,
            IChatLogsRepository chatLogsRepository)
        {
            _gameServersRepository = gameServersRepository;
            _playersRepository = playersRepository;
            _chatLogsRepository = chatLogsRepository;
        }

        [FunctionName("ProcessChatMessage")]
        public async Task RunProcessChatMessage([ServiceBusTrigger("chat-message", Connection = "ServiceBus:ServiceBusConnectionString")]
            string myQueueItem, ILogger log)
        {
            try
            {
                var chatMessage = JsonConvert.DeserializeObject<ChatMessage>(myQueueItem);

                if (chatMessage == null)
                {
                    log.LogError($"Could not process chat message: {myQueueItem}");
                }
                else
                {
                    var server = await _gameServersRepository.GetGameServer(chatMessage.ServerId);

                    var databasePlayer = await _playersRepository.GetPlayer(server.GameType, chatMessage.Guid);
                    if (databasePlayer == null)
                    {
                        log.LogWarning($"Could not handle chat message {chatMessage.Message} for {chatMessage.Username} as player is null for {chatMessage.Guid} on server {chatMessage.ServerId}");
                        return;
                    }

                    log.LogInformation($"[{server.Title}] {chatMessage.Username} :: {chatMessage.Message}");

                    var cleanedMessage = chatMessage.Message.Replace("\u0014", "");

                    await _chatLogsRepository.CreateChatLog(new ChatLogDto
                    {
                        PlayerId = databasePlayer.PlayerId,
                        ServerId = chatMessage.ServerId,
                        Username = chatMessage.Username,
                        ChatType = chatMessage.ChatType.ToString(),
                        Message = cleanedMessage
                    });
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Failed to process chat message: {myQueueItem}");
            }
        }
    }
}