using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using XI.Portal.Players.Interfaces;
using XI.Portal.Servers.Interfaces;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XI.Portal.FuncApp
{
    // ReSharper disable once UnusedMember.Global
    public class DataMaintenance
    {
        private readonly IGameServerStatusStatsRepository _gameServerStatusStatsRepository;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IPlayerLocationsRepository _playerLocationsRepository;
        private readonly IPlayersCacheRepository _playersCache;

        public DataMaintenance(
            IPlayerLocationsRepository playerLocationsRepository,
            IPlayersCacheRepository playersCache,
            IGameServerStatusStatsRepository gameServerStatusStatsRepository,
            IRepositoryApiClient repositoryApiClient)
        {
            _playerLocationsRepository = playerLocationsRepository ?? throw new ArgumentNullException(nameof(playerLocationsRepository));
            _playersCache = playersCache ?? throw new ArgumentNullException(nameof(playersCache));
            _gameServerStatusStatsRepository = gameServerStatusStatsRepository;

            this.repositoryApiClient = repositoryApiClient;
        }

        [FunctionName("DataMaintenance")]
        // ReSharper disable once UnusedMember.Global
        public async Task RunDataMaintenance([TimerTrigger("0 0 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogDebug($"Start RunDataMaintenance @ {DateTime.UtcNow}");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var servers = await repositoryApiClient.GameServers.GetGameServers(null, null, null, 0, 0, null);
            var serverIds = servers.Select(s => s.Id).ToList();

            await _playerLocationsRepository.RemoveOldEntries(serverIds);
            await _playersCache.RemoveOldEntries();
            await _gameServerStatusStatsRepository.RemoveOldEntries(serverIds);

            stopWatch.Stop();
            log.LogDebug($"Stop RunDataMaintenance @ {DateTime.UtcNow} after {stopWatch.ElapsedMilliseconds} milliseconds");
        }
    }
}