using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using XI.CommonTypes;
using XI.Portal.Bus.Models;
using XI.Portal.Data.Legacy;
using XI.Portal.Repository.Interfaces;

namespace XI.Portal.FuncApp
{
    public class MapVoting
    {
        private readonly LegacyPortalContext _legacyPortalContext;
        private readonly IMapVotesRepository _mapVotesRepository;

        public MapVoting(
            IMapVotesRepository mapVotesRepository,
            LegacyPortalContext legacyPortalContext)
        {
            _mapVotesRepository = mapVotesRepository;
            _legacyPortalContext = legacyPortalContext;
        }

        [FunctionName("MapVoting")]
        public async Task RunMapVoting([ServiceBusTrigger("map-votes", Connection = "ServiceBus:ServiceBusConnectionString")]
            string myQueueItem, ILogger log)
        {
            var mapVote = JsonConvert.DeserializeObject<MapVote>(myQueueItem);

            if (mapVote == null)
            {
                log.LogError($"Could not process map vote: {myQueueItem}");
            }
            else
            {
                log.LogInformation($"Updating map vote {mapVote.GameType} - {mapVote.MapName} for {mapVote.Guid} as {mapVote.Like}");
                await _mapVotesRepository.UpdateMapVote(mapVote);
            }
        }

        [FunctionName("MapVotingRebuildIndex")]
        public async Task RunMapVotingRebuildIndex([TimerTrigger("0 */30 * * * *")] TimerInfo myTimer, ILogger log)
        {
            var maps = await _legacyPortalContext.Maps.ToListAsync();
            await _mapVotesRepository.RebuildIndex(maps.ToDictionary(m => m.GameType, m => m.MapName));
        }
    }
}