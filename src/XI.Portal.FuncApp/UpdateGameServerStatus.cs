using System;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using XI.Portal.Data.Legacy;

namespace XI.Portal.FuncApp
{
    public class UpdateGameServerStatus
    {
        private readonly LegacyPortalContext _legacyContext;

        public UpdateGameServerStatus(LegacyPortalContext legacyContext)
        {
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
        }

        [FunctionName("UpdateGameServerStatus")]
        public void Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            foreach (var server in _legacyContext.GameServers.Where(server => server.ShowOnPortalServerList)) log.LogInformation("Updating game server status for {Title}", server.Title);
        }
    }
}