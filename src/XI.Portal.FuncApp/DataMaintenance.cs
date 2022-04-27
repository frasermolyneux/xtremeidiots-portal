using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using XI.Portal.Players.Interfaces;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;

namespace XI.Portal.FuncApp
{
    // ReSharper disable once UnusedMember.Global
    public class DataMaintenance
    {
        private readonly IGameServersRepository _gameServersRepository;
        private readonly IChatLogsRepository _chatLogsRepository;
        private readonly IGameServerStatusStatsRepository _gameServerStatusStatsRepository;
        private readonly IPlayerLocationsRepository _playerLocationsRepository;
        private readonly IPlayersCacheRepository _playersCache;

        public DataMaintenance(
            IPlayerLocationsRepository playerLocationsRepository,
            IPlayersCacheRepository playersCache,
            IGameServerStatusStatsRepository gameServerStatusStatsRepository,
            IGameServersRepository gameServersRepository,
            IChatLogsRepository chatLogsRepository)
        {
            _playerLocationsRepository = playerLocationsRepository ?? throw new ArgumentNullException(nameof(playerLocationsRepository));
            _playersCache = playersCache ?? throw new ArgumentNullException(nameof(playersCache));
            _gameServerStatusStatsRepository = gameServerStatusStatsRepository;
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
            _chatLogsRepository = chatLogsRepository ?? throw new ArgumentNullException(nameof(chatLogsRepository));
        }

        [FunctionName("DataMaintenance")]
        // ReSharper disable once UnusedMember.Global
        public async Task RunDataMaintenance([TimerTrigger("0 0 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogDebug($"Start RunDataMaintenance @ {DateTime.UtcNow}");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var servers = await _gameServersRepository.GetGameServers(new GameServerFilterModel { Filter = GameServerFilterModel.FilterBy.None });
            var serverIds = servers.Select(s => s.ServerId).ToList();

            await _playerLocationsRepository.RemoveOldEntries(serverIds);
            await _playersCache.RemoveOldEntries();
            await _gameServerStatusStatsRepository.RemoveOldEntries(serverIds);

            stopWatch.Stop();
            log.LogDebug($"Stop RunDataMaintenance @ {DateTime.UtcNow} after {stopWatch.ElapsedMilliseconds} milliseconds");
        }
    }
}