using System;
using System.Threading.Tasks;
using XI.CommonTypes;
using XI.Portal.Bus.Client;

namespace XI.Portal.Bot.ExecMapVote
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
            var gameTypeArg = args[0];
            var mapName = args[1];
            var guid = args[2];
            var likeArg = args[3];

            var gameType = Enum.Parse<GameType>(gameTypeArg);
            var like = Convert.ToBoolean(likeArg);

            await _portalServiceBusClient.PostMapVote(new Bus.Models.MapVote
            {
                GameType = gameType,
                MapName = mapName,
                Guid = guid,
                Like = like
            });
        }
    }
}