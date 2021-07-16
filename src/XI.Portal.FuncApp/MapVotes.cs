using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace XI.Portal.FuncApp
{
    public static class MapVotes
    {
        [FunctionName("MapVotes")]
        public static void Run([ServiceBusTrigger("map-votes", Connection = "ServiceBusQueueConnectionString")]
            string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
        }
    }
}