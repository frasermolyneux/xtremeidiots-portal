using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using XI.AzureTableLogging.Logger;
using XI.Portal.Players.Interfaces;

namespace XI.Portal.FuncApp
{
    // ReSharper disable once UnusedMember.Global
    public class DataMaintenance
    {
        private readonly IPlayerLocationsRepository _playerLocationsRepository;
        private readonly IPlayersCacheRepository _playersCache;
        private readonly AzureTableLogger _azureTableLogger;

        public DataMaintenance(IPlayerLocationsRepository playerLocationsRepository, IPlayersCacheRepository playersCache, AzureTableLogger azureTableLogger)
        {
            _playerLocationsRepository = playerLocationsRepository ?? throw new ArgumentNullException(nameof(playerLocationsRepository));
            _playersCache = playersCache ?? throw new ArgumentNullException(nameof(playersCache));
            _azureTableLogger = azureTableLogger ?? throw new ArgumentNullException(nameof(azureTableLogger));
        }

        [FunctionName("DataMaintenance")]
        // ReSharper disable once UnusedMember.Global
        public async Task RunDataMaintenance([TimerTrigger("0 0 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogDebug($"Start RunDataMaintenance @ {DateTime.UtcNow}");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            await _playerLocationsRepository.RemoveOldEntries();
            await _playersCache.RemoveOldEntries();
            await _azureTableLogger.RemoveOldEntries(48);

            stopWatch.Stop();
            log.LogDebug($"Stop RunDataMaintenance @ {DateTime.UtcNow} after {stopWatch.ElapsedMilliseconds} milliseconds");
        }
    }
}