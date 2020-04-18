using System;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using XI.Portal.Data.Legacy;
using XI.Servers.Factories;

namespace XI.Portal.FuncApp
{
    public class UpdateGameServerStatus
    {
        private readonly IGameServerStatusHelperFactory _gameServerStatusHelperFactory;
        private readonly LegacyPortalContext _legacyContext;

        public UpdateGameServerStatus(LegacyPortalContext legacyContext, IGameServerStatusHelperFactory gameServerStatusHelperFactory)
        {
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
            _gameServerStatusHelperFactory = gameServerStatusHelperFactory ?? throw new ArgumentNullException(nameof(gameServerStatusHelperFactory));
        }

        [FunctionName("UpdateGameServerStatus")]
        public async void Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            foreach (var server in _legacyContext.GameServers.Where(server => server.ShowOnPortalServerList))
            {
                log.LogInformation("Updating game server status for {Title}", server.Title);

                try
                {
                    var gameServerStatusHelper = _gameServerStatusHelperFactory.GetGameServerStatusHelper(server.GameType, server.Title, server.Hostname, server.QueryPort, server.RconPassword);
                    var gameServerStatus = await gameServerStatusHelper.GetServerStatus();

                    log.LogInformation($"{gameServerStatus.ServerName} is online running {gameServerStatus.Map} with {gameServerStatus.PlayerCount} players connected");
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Failed to get game server status for {Title}", server.Title);
                }
            }
        }
    }
}