using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.AdminWebApp.Auth.Constants;
using XtremeIdiots.Portal.AdminWebApp.Models;
using XtremeIdiots.Portal.AdminWebApp.ViewModels;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Maps;
using XtremeIdiots.Portal.RepositoryApiClient;
using XtremeIdiots.Portal.ServersApiClient;

namespace XtremeIdiots.Portal.AdminWebApp.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessGameServers)]
    public class MapManagerController : Controller
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IServersApiClient serversApiClient;

        public MapManagerController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            IServersApiClient serversApiClient)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.serversApiClient = serversApiClient ?? throw new ArgumentNullException(nameof(serversApiClient));
        }

        [HttpGet]
        public async Task<IActionResult> Manage(Guid id)
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.GetGameServer(id);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return NotFound();

            var canManageGameServerMaps = await authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.GameType, AuthPolicies.ManageMaps);

            if (!canManageGameServerMaps.Succeeded)
                return Unauthorized();

            var gameServerStatsResponseDto = await repositoryApiClient.GameServersStats.GetGameServerStatusStats(gameServerApiResponse.Result.GameServerId, DateTime.UtcNow.AddDays(-2));

            var mapTimelineDataPoints = new List<MapTimelineDataPoint>();
            var maps = new List<MapDto>();

            if (gameServerStatsResponseDto.IsSuccess && gameServerStatsResponseDto.Result != null)
            {
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
                var mapsCollectionApiResponse = await repositoryApiClient.Maps.GetMaps(gameServerApiResponse.Result.GameType, mapNames, null, null, 0, 50, MapsOrder.MapNameAsc);

                if (mapsCollectionApiResponse.Result != null)
                    maps = mapsCollectionApiResponse.Result.Entries;
            }

            var ftpMaps = await serversApiClient.Maps.GetLoadedServerMapsFromHost(id);

            var viewModel = new ManageMapsViewModel(gameServerApiResponse.Result)
            {
                Maps = maps,
                MapTimelineDataPoints = mapTimelineDataPoints,
                ServerMaps = ftpMaps.Result.Entries.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> PushMapToRemote(PushMapToRemoteViewModel viewModel)
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.GetGameServer(viewModel.GameServerId);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return NotFound();

            var canManageGameServerMaps = await authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.GameType, AuthPolicies.ManageMaps);

            if (!canManageGameServerMaps.Succeeded)
                return Unauthorized();

            await serversApiClient.Maps.PushServerMapToHost(gameServerApiResponse.Result.GameServerId, viewModel.MapName);

            return RedirectToAction("Manage", new { id = gameServerApiResponse.Result.GameServerId });
        }
    }
}
