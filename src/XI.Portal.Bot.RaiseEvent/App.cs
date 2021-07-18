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
            }
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