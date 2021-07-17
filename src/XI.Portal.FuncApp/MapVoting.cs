using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using XI.Portal.Bus.Models;
using XI.Portal.Repository.Dtos;
using XI.Portal.Repository.Interfaces;

namespace XI.Portal.FuncApp
{
    public class MapVoting
    {
        private readonly IMapsRepository _mapsRepository;

        public MapVoting(
            IMapsRepository mapsRepository)
        {
            _mapsRepository = mapsRepository;
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
                await _mapsRepository.InsertOrMergeMapVote(new MapVoteDto
                {
                    GameType = mapVote.GameType,
                    MapName = mapVote.MapName,
                    Guid = mapVote.Guid,
                    Like = mapVote.Like
                });
            }
        }

        [FunctionName("MapVotingRebuildIndex")]
        public async Task RunMapVotingRebuildIndex([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation("Rebuilding map votes");
            await _mapsRepository.RebuildMapVotes();
        }
    }
}