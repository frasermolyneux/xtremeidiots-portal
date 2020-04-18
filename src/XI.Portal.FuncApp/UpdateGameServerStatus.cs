using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace XI.Portal.FuncApp
{
    public static class UpdateGameServerStatus
    {
        [FunctionName("UpdateGameServerStatus")]
        public static void Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}