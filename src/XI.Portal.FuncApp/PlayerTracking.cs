using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace XI.Portal.FuncApp
{
    internal class PlayerTracking
    {
        [FunctionName("ProcessPlayerAuth")]
        public async Task RunProcessPlayerAuth([ServiceBusTrigger("player-auth", Connection = "ServiceBus:ServiceBusConnectionString")]
            string myQueueItem, ILogger log)
        {
            log.LogInformation($"Processing player auth: {myQueueItem}");
        }
    }
}