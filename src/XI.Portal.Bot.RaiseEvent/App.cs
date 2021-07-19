using System;
using System.Threading.Tasks;
using XI.CommonTypes;
using XI.Portal.Bus.Client;
using XI.Portal.Bus.Models;

namespace XI.Portal.Bot.RaiseEvent
{
    internal class App
    {
        private readonly IPortalServiceBusClient _portalServiceBusClient;

        public App(IPortalServiceBusClient portalServiceBusClient)
        {
            _portalServiceBusClient = portalServiceBusClient;
        }

        public async Task Run(string[] args)
        {
            var eventType = args[0];

            switch (eventType)
            {
                case "mapvote":
                    await RaiseMapVoteEvent(Enum.Parse<GameType>(args[1]), args[2], args[3], Convert.ToBoolean(args[4]));
                    break;
                case "playerauth":
                    await RaisePlayerAuthEvent(Enum.Parse<GameType>(args[1]), Guid.Parse(args[2]), args[3], args[4], args[5]);
                    break;
                case "chatmessage":
                    await RaiseChatMessageEvent(Enum.Parse<GameType>(args[1]), Guid.Parse(args[2]), args[3], args[4], Enum.Parse<ChatType>(args[5]), args[6]);
                    break;
            }
        }

        private async Task RaiseChatMessageEvent(GameType gameType, Guid serverId, string guid, string username, ChatType chatType, string message)
        {
            await _portalServiceBusClient.PostChatMessageEvent(new ChatMessage
            {
                GameType = gameType,
                ServerId = serverId,
                ChatType = chatType,
                Guid = guid,
                Username = username,
                Message = message,
                Time = DateTime.UtcNow
            });
        }

        private async Task RaisePlayerAuthEvent(GameType gameType, Guid serverId, string guid, string username, string ipAddress)
        {
            await _portalServiceBusClient.PostPlayerAuth(new PlayerAuth
            {
                GameType = gameType,
                ServerId = serverId,
                Guid = guid,
                Username = username,
                IpAddress = ipAddress
            });
        }

        public async Task RaiseMapVoteEvent(GameType gameType, string mapName, string guid, bool like)
        {
            await _portalServiceBusClient.PostMapVote(new MapVote
            {
                GameType = gameType,
                MapName = mapName,
                Guid = guid,
                Like = like
            });
        }
    }
}