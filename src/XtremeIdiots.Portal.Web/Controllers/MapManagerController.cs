using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.ViewModels;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApiClient.V1;
using XtremeIdiots.Portal.ServersApiClient;

namespace XtremeIdiots.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.ManageMaps)]
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
            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return NotFound();

            var canManageGameServerMaps = await authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.GameType, AuthPolicies.ManageMaps);

            if (!canManageGameServerMaps.Succeeded)
                return Unauthorized();

            var rconMaps = await serversApiClient.Rcon.GetServerMaps(id);
            var ftpMaps = await serversApiClient.Maps.GetLoadedServerMapsFromHost(id);
            var mapPacks = await repositoryApiClient.MapPacks.V1.GetMapPacks(null, [id], null, 0, 50, MapPacksOrder.Title);

            var mapsCollectionApiResponse = await repositoryApiClient.Maps.V1.GetMaps(gameServerApiResponse.Result.GameType, rconMaps.Result?.Entries.Select(m => m.MapName).ToArray(), null, null, 0, 50, MapsOrder.MapNameAsc);

            var viewModel = new ManageMapsViewModel(gameServerApiResponse.Result)
            {
                Maps = mapsCollectionApiResponse.Result.Entries,
                ServerMaps = ftpMaps.Result.Entries,
                RconMaps = rconMaps.Result.Entries,
                MapPacks = mapPacks.Result.Entries
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> PushMapToRemote(PushMapToRemoteViewModel viewModel)
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(viewModel.GameServerId);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return NotFound();

            var canManageGameServerMaps = await authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.GameType, AuthPolicies.ManageMaps);

            if (!canManageGameServerMaps.Succeeded)
                return Unauthorized();

            await serversApiClient.Maps.PushServerMapToHost(gameServerApiResponse.Result.GameServerId, viewModel.MapName);

            return RedirectToAction("Manage", new { id = gameServerApiResponse.Result.GameServerId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMapFromHost(DeleteMapFromHostModel model)
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(model.GameServerId);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return NotFound();

            var canManageGameServerMaps = await authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.GameType, AuthPolicies.ManageMaps);

            if (!canManageGameServerMaps.Succeeded)
                return Unauthorized();

            await serversApiClient.Maps.DeleteServerMapFromHost(model.GameServerId, model.MapName);

            return RedirectToAction("Manage", new { id = model.GameServerId });
        }
    }
}
