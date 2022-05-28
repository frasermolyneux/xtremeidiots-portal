using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Net;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XtremeIdiots.Portal.SyncFunc
{
    public class MapImageSync
    {
        private readonly ILogger<MapImageSync> logger;
        private readonly IRepositoryApiClient repositoryApiClient;

        public MapImageSync(
            ILogger<MapImageSync> logger,
            IRepositoryApiClient repositoryApiClient)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
        }

        [FunctionName("MapImageSync")]
        public async Task RunMapImageSync([TimerTrigger("0 0 0 * * 3")] TimerInfo myTimer, ILogger log)
        {
            var gamesToSync = new Dictionary<GameType, string>
            {
                {GameType.CallOfDuty2, "cod2"},
                {GameType.CallOfDuty4, "cod4"},
                {GameType.CallOfDuty5, "codww"},
                {GameType.UnrealTournament2004, "ut2k4"},
                {GameType.Insurgency, "ins"},
            };

            foreach (var game in gamesToSync)
            {
                var mapsResponseDto = await repositoryApiClient.Maps.GetMaps(game.Key, null, null, null, null, null);

                if (mapsResponseDto == null)
                {
                    logger.LogCritical($"Failed to retrieve maps from repository for game type '{game}'");
                    continue;
                }

                var mapsToUpdate = mapsResponseDto.Entries.Where(m => string.IsNullOrWhiteSpace(m.MapImageUri)).ToList();

                log.LogInformation($"Total maps retrieved from redirect for {game.Key} is {mapsResponseDto.Entries.Count} with {mapsToUpdate.Count} needing updating");

                foreach (var mapDto in mapsToUpdate)
                {
                    var gameTrackerImageUrl = $"https://image.gametracker.com/images/maps/160x120/{game.Value}/{mapDto.MapName}.jpg";

                    try
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                        using (var client = new WebClient())
                        {
                            client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");

                            var filePath = Path.GetTempFileName();
                            client.DownloadFile(new Uri(gameTrackerImageUrl), filePath);

                            await repositoryApiClient.Maps.UpdateMapImage(mapDto.MapId, filePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.LogWarning(ex, $"Failed to retrieve map image from {gameTrackerImageUrl}");
                    }
                }
            }
        }
    }
}