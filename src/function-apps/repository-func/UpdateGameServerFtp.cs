using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;

namespace XtremeIdiots.Portal.RepositoryFunc;

public class UpdateGameServerFtp
{
    [FunctionName("UpdateGameServerFtp")]
    public void Run([TimerTrigger("0 0 * * * *")] TimerInfo myTimer, ILogger log)
    {
        log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
    }
}