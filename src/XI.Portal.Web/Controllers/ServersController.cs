using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using XI.Portal.Web.Auth.Constants;
using XI.Portal.Web.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApiClient;
using XtremeIdiots.Portal.ServersApiClient;

namespace XI.Portal.Web.Controllers
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
            var livePlayersResponseDto = await repositoryApiClient.LivePlayers.GetLivePlayers(null, null, LivePlayerFilter.GeoLocated);

            return View(livePlayersResponseDto.Entries);
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

            return View(new ServersGameServerViewModel
            {
                GameServer = gameServerDto,
                Map = mapDto,
                Maps = mapsResponseDto.Entries,
                GameServerStats = gameServerStatusStats,
                LivePlayers = livePlayersResponseDto.Entries
            });
        }
    }
}