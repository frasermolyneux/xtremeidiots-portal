using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.AdminWebApp.Auth.Constants;
using XtremeIdiots.Portal.AdminWebApp.Models;
using XtremeIdiots.Portal.AdminWebApp.ViewModels;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;
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
            var gameServersApiResponse = await repositoryApiClient.GameServers.GetGameServers(null, null, GameServerFilter.ShowOnPortalServerList, 0, 50, GameServerOrder.BannerServerListPosition);

            var result = gameServersApiResponse.Result.Entries.Select(gs => new ServersGameServerViewModel(gs)).ToList();

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Map()
        {
            var response = await repositoryApiClient.RecentPlayers.GetRecentPlayers(null, null, DateTime.UtcNow.AddHours(-48), RecentPlayersFilter.GeoLocated, 0, 200, null);
            return View(response.Result?.Entries);
        }

        [HttpGet]
        public async Task<IActionResult> ServerInfo(Guid id)
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.GetGameServer(id);

            if (gameServerApiResponse.IsNotFound)
                return NotFound();

            var gameServerStatsResponseDto = await repositoryApiClient.GameServersStats.GetGameServerStatusStats(gameServerApiResponse.Result.Id, DateTime.UtcNow.AddDays(-2));
            var livePlayersResponseDto = await repositoryApiClient.LivePlayers.GetLivePlayers(null, gameServerApiResponse.Result.Id, LivePlayerFilter.GeoLocated, 0, 50, LivePlayersOrder.ScoreDesc);

            var mapApiResponse = await repositoryApiClient.Maps.GetMap(gameServerApiResponse.Result.GameType, gameServerApiResponse.Result.LiveMap);
            var mapDto = mapApiResponse.Result ?? null;

            var mapNames = gameServerStatsResponseDto.Result.Entries.GroupBy(m => m.MapName).Select(m => m.Key).ToArray();
            var mapsCollectionApiResponse = await repositoryApiClient.Maps.GetMaps(gameServerApiResponse.Result.GameType, mapNames, null, null, 0, 50, MapsOrder.MapNameAsc);

            var mapTimelineDataPoints = new List<MapTimelineDataPoint>();

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

            return View(new ServersGameServerViewModel(gameServerApiResponse.Result)
            {
                Map = mapDto,
                Maps = mapsCollectionApiResponse.Result.Entries,
                GameServerStats = gameServerStatsResponseDto.Result.Entries,
                LivePlayers = livePlayersResponseDto.Result?.Entries != null ? livePlayersResponseDto.Result.Entries : new List<LivePlayerDto>(),
                MapTimelineDataPoints = mapTimelineDataPoints
            });
        }
    }
}