using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using XtremeIdiots.Portal.AdminWebApp.Auth.Constants;
using XtremeIdiots.Portal.AdminWebApp.ViewModels;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.MapPacks;
using XtremeIdiots.Portal.RepositoryApiClient.V1;
using XtremeIdiots.Portal.ServersApiClient;

namespace XtremeIdiots.Portal.AdminWebApp.Controllers
{
    [Authorize(Policy = AuthPolicies.ManageMaps)]
    public class MapPacksController : Controller
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IServersApiClient serversApiClient;

        public MapPacksController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            IServersApiClient serversApiClient)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.serversApiClient = serversApiClient ?? throw new ArgumentNullException(nameof(serversApiClient));
        }

        [HttpGet]
        public async Task<IActionResult> Create(Guid gameServerId)
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(gameServerId);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return NotFound();

            var canManageGameServerMaps = await authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.GameType, AuthPolicies.ManageMaps);

            if (!canManageGameServerMaps.Succeeded)
                return Unauthorized();

            ViewData["GameServer"] = gameServerApiResponse.Result;

            return View(new CreateMapPackViewModel() { GameServerId = gameServerId });
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateMapPackViewModel createMapPackViewModel)
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(createMapPackViewModel.GameServerId);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return NotFound();

            var canManageGameServerMaps = await authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.GameType, AuthPolicies.ManageMaps);

            if (!canManageGameServerMaps.Succeeded)
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                ViewData["GameServer"] = gameServerApiResponse.Result;
                return View(createMapPackViewModel);
            }

            var createMapPackDto = new CreateMapPackDto(createMapPackViewModel.GameServerId, createMapPackViewModel.Title, createMapPackViewModel.Description)
            {
                GameMode = createMapPackViewModel.GameMode,
                SyncToGameServer = createMapPackViewModel.SyncToGameServer
            };

            var createMapPackApiResponse = await repositoryApiClient.MapPacks.V1.CreateMapPack(createMapPackDto);

            if (!createMapPackApiResponse.IsSuccess)
            {
                createMapPackApiResponse.Errors.ForEach(error => ModelState.AddModelError(string.Empty, error));
                ViewData["GameServer"] = gameServerApiResponse.Result;
                return View(createMapPackViewModel);
            }

            return RedirectToAction("Manage", "MapPacks", new { id = createMapPackViewModel.GameServerId });
        }
    }
}