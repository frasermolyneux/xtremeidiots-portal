using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace XI.Portal.FuncApp
{
    internal class ChatMessaging
    {
        [FunctionName("ProcessChatMessage")]
        public async Task RunProcessChatMessage([ServiceBusTrigger("chat-message", Connection = "ServiceBus:ServiceBusConnectionString")]
            string myQueueItem, ILogger log)
        {
            log.LogInformation($"Processing chat message: {myQueueItem}");
        }
    }
}