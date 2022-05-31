using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XtremeIdiots.Portal.AdminWebApp.Auth.Constants;
using XtremeIdiots.Portal.AdminWebApp.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApiClient;
using XtremeIdiots.Portal.ServersApiClient;

namespace XtremeIdiots.Portal.AdminWebApp.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessServers)]
    public class ServersController : Controller
    {
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IServersApiClient serversApiClient;

        public ServersController(
            IRepositoryApiClient repositoryApiClient,
            IServersApiClient serversApiClient)
        {
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.serversApiClient = serversApiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var gameServerDtos = await repositoryApiClient.GameServers.GetGameServers(null, null, GameServerFilter.ShowOnPortalServerList, 0, 0, GameServerOrder.BannerServerListPosition);

            var result = gameServerDtos.Select(gs => new ServersGameServerViewModel
            {
                GameServer = gs
            }).ToList();

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Map()
        {
            var recentPlayersCollectionDto = await repositoryApiClient.RecentPlayers.GetRecentPlayers(null, null, DateTime.UtcNow.AddHours(-48), RecentPlayersFilter.GeoLocated, 0, 200, null);

            return View(recentPlayersCollectionDto?.Entries);
        }

        [HttpGet]
        public async Task<IActionResult> ServerInfo(Guid id)
        {
            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(id);

            if (gameServerDto == null)
                return NotFound();

            var serverQueryStatusResponseStatusDto = await serversApiClient.Query.GetServerStatus(gameServerDto.Id);
            var gameServerStatusStats = await repositoryApiClient.GameServersStats.GetGameServerStatusStats(gameServerDto.Id, DateTime.UtcNow.AddDays(-2));
            var livePlayersResponseDto = await repositoryApiClient.LivePlayers.GetLivePlayers(null, gameServerDto.Id, LivePlayerFilter.GeoLocated);

            MapDto mapDto = null;
            if (!string.IsNullOrWhiteSpace(serverQueryStatusResponseStatusDto.Map))
                mapDto = await repositoryApiClient.Maps.GetMap(gameServerDto.GameType, serverQueryStatusResponseStatusDto.Map);

            var mapNames = gameServerStatusStats.GroupBy(m => m.MapName).Select(m => m.Key).ToArray();
            var mapsResponseDto = await repositoryApiClient.Maps.GetMaps(gameServerDto.GameType, mapNames, null, null, null, MapsOrder.MapNameAsc);

            var mapTimelineDataPoints = new List<MapTimelineDataPoint>();

            GameServerStatDto? current = null;
            foreach (var gameServerStatusStatDto in gameServerStatusStats.OrderBy(gss => gss.Timestamp))
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

                if (current == gameServerStatusStats.Last())
                    mapTimelineDataPoints.Add(new MapTimelineDataPoint(current.MapName, current.Timestamp, DateTime.UtcNow));
            }

            return View(new ServersGameServerViewModel
            {
                GameServer = gameServerDto,
                Map = mapDto,
                Maps = mapsResponseDto.Entries,
                GameServerStats = gameServerStatusStats,
                LivePlayers = livePlayersResponseDto.Entries,
                MapTimelineDataPoints = mapTimelineDataPoints
            });
        }
    }
}