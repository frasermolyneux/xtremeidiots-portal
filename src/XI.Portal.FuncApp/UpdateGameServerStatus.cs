using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XI.Portal.Data.Legacy;
using XI.Portal.Players.Interfaces;
using XI.Portal.Servers.Interfaces;

namespace XI.Portal.FuncApp
{
    public class UpdateGameServerStatus
    {
        private readonly IGameServerStatusRepository _gameServerStatusRepository;
        private readonly LegacyPortalContext _legacyContext;
        private readonly IPlayerLocationsRepository _playerLocationsRepository;

        public UpdateGameServerStatus(LegacyPortalContext legacyContext, IGameServerStatusRepository gameServerStatusRepository, IPlayerLocationsRepository playerLocationsRepository)
        {
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
            _gameServerStatusRepository = gameServerStatusRepository ?? throw new ArgumentNullException(nameof(gameServerStatusRepository));
            _playerLocationsRepository = playerLocationsRepository ?? throw new ArgumentNullException(nameof(playerLocationsRepository));
        }

        [FunctionName("UpdateGameServerStatus")]
        public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var servers = await _legacyContext.GameServers.Where(server => server.ShowOnPortalServerList).ToListAsync();

            foreach (var server in servers)
            {
                log.LogInformation("Updating game server status for {Title}", server.Title);

                try
                {
                    var model = await _gameServerStatusRepository.GetStatus(server.ServerId, null, null, TimeSpan.FromMinutes(-10));
                    log.LogInformation($"{model.ServerName} is online running {model.Map} with {model.PlayerCount} players connected");
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Failed to get game server status for {Title}", server.Title);
                }
            }
        }
    }
}