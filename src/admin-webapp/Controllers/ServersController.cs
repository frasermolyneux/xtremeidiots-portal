using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.AdminWebApp.Auth.Constants;
using XtremeIdiots.Portal.AdminWebApp.Models;
using XtremeIdiots.Portal.AdminWebApp.ViewModels;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Maps;
using XtremeIdiots.Portal.RepositoryApiClient.V1;

namespace XtremeIdiots.Portal.AdminWebApp.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessServers)]
    public class ServersController : Controller
    {
        private readonly IRepositoryApiClient repositoryApiClient;

        public ServersController(
            IRepositoryApiClient repositoryApiClient)
        {
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var gameServersApiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(null, null, GameServerFilter.PortalServerListEnabled, 0, 50, GameServerOrder.BannerServerListPosition);

            if (!gameServersApiResponse.IsSuccess || gameServersApiResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            var result = gameServersApiResponse.Result.Entries.Select(gs => new ServersGameServerViewModel(gs)).ToList();

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Map()
        {
            var response = await repositoryApiClient.RecentPlayers.V1.GetRecentPlayers(null, null, DateTime.UtcNow.AddHours(-48), RecentPlayersFilter.GeoLocated, 0, 200, null);
            return View(response.Result?.Entries);
        }

        [HttpGet]
        public async Task<IActionResult> ServerInfo(Guid id)
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return NotFound();

            MapDto? mapDto = null;
            if (!string.IsNullOrWhiteSpace(gameServerApiResponse.Result.LiveMap))
            {
                var mapApiResponse = await repositoryApiClient.Maps.V1.GetMap(gameServerApiResponse.Result.GameType, gameServerApiResponse.Result.LiveMap);
                mapDto = mapApiResponse.Result ?? null;
            }

            var gameServerStatsResponseDto = await repositoryApiClient.GameServersStats.V1.GetGameServerStatusStats(gameServerApiResponse.Result.GameServerId, DateTime.UtcNow.AddDays(-2));

            var mapTimelineDataPoints = new List<MapTimelineDataPoint>();
            var gameServerStatDtos = new List<GameServerStatDto>();
            var maps = new List<MapDto>();

            if (gameServerStatsResponseDto.IsSuccess && gameServerStatsResponseDto.Result != null)
            {
                gameServerStatDtos = gameServerStatsResponseDto.Result.Entries;

                GameServerStatDto? current = null;
                foreach (var gameServerStatusStatDto in gameServerStatsResponseDto.Result.Entries.OrderBy(gss => gss.Timestamp))
                {
                    if (current == null)
                    {
                        current = gameServerStatusStatDto;
                        continue;
                    }

                    if (current.MapName != gameServerStatusStatDto.MapName)
                    {
                        mapTimelineDataPoints.Add(new MapTimelineDataPoint(current.MapName, current.Timestamp, gameServerStatusStatDto.Timestamp));
                        current = gameServerStatusStatDto;
                        continue;
                    }

                    if (current == gameServerStatsResponseDto.Result.Entries.Last())
                        mapTimelineDataPoints.Add(new MapTimelineDataPoint(current.MapName, current.Timestamp, DateTime.UtcNow));
                }

                var mapNames = gameServerStatsResponseDto.Result.Entries.GroupBy(m => m.MapName).Select(m => m.Key).ToArray();
                var mapsCollectionApiResponse = await repositoryApiClient.Maps.V1.GetMaps(gameServerApiResponse.Result.GameType, mapNames, null, null, 0, 50, MapsOrder.MapNameAsc);

                if (mapsCollectionApiResponse.Result != null)
                    maps = mapsCollectionApiResponse.Result.Entries;
            }

            return View(new ServersGameServerViewModel(gameServerApiResponse.Result)
            {
                Map = mapDto,
                Maps = maps,
                GameServerStats = gameServerStatDtos,
                MapTimelineDataPoints = mapTimelineDataPoints
            });
        }
    }
}