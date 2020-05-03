using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using XI.Portal.Players.Interfaces;

namespace XI.Portal.FuncApp
{
    // ReSharper disable once UnusedMember.Global
    public class DataMaintenance
    {
        private readonly IPlayerLocationsRepository _playerLocationsRepository;
        private readonly IPlayersCacheRepository _playersCache;

        public DataMaintenance(IPlayerLocationsRepository playerLocationsRepository, IPlayersCacheRepository playersCache)
        {
            _playerLocationsRepository = playerLocationsRepository ?? throw new ArgumentNullException(nameof(playerLocationsRepository));
            _playersCache = playersCache ?? throw new ArgumentNullException(nameof(playersCache));
        }

        [FunctionName("DataMaintenance")]
        public async Task Run([TimerTrigger("0 0 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Starting player locations maintenance: {DateTime.Now}");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            await _playerLocationsRepository.RemoveOldEntries();
            await _playersCache.RemoveOldEntries();

            stopWatch.Stop();
            log.LogInformation($"C# Timer completed after {stopWatch.ElapsedMilliseconds} milliseconds");
        }
    }
}