using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using XI.Portal.Players.Interfaces;

namespace XI.Portal.FuncApp
{
    public class PlayerLocationsMaintenance
    {
        private readonly IPlayerLocationsRepository _playerLocationsRepository;

        public PlayerLocationsMaintenance(IPlayerLocationsRepository playerLocationsRepository)
        {
            _playerLocationsRepository = playerLocationsRepository ?? throw new ArgumentNullException(nameof(playerLocationsRepository));
        }

        [FunctionName("PlayerLocationsMaintenance")]
        public async Task Run([TimerTrigger("0 0 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Starting player locations maintenance: {DateTime.Now}");
            await _playerLocationsRepository.RemoveOldEntries();
        }
    }
}