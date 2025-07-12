using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.ViewModels;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.MapPacks;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Integrations.Servers.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
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

            var canManageGameServerMaps = await authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.Data.GameType, AuthPolicies.ManageMaps);

            if (!canManageGameServerMaps.Succeeded)
                return Unauthorized();

            ViewData["GameServer"] = gameServerApiResponse.Result.Data;

            return View(new CreateMapPackViewModel() { GameServerId = gameServerId });
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateMapPackViewModel createMapPackViewModel)
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(createMapPackViewModel.GameServerId);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return NotFound();

            var canManageGameServerMaps = await authorizationService.AuthorizeAsync(User, gameServerApiResponse.Result.Data.GameType, AuthPolicies.ManageMaps);

            if (!canManageGameServerMaps.Succeeded)
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                ViewData["GameServer"] = gameServerApiResponse.Result.Data;
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
                createMapPackApiResponse.Result.Errors.ToList().ForEach(error => ModelState.AddModelError(error.Target, error.Message));
                ViewData["GameServer"] = gameServerApiResponse.Result.Data;
                return View(createMapPackViewModel);
            }

            return RedirectToAction("Manage", "MapPacks", new { id = createMapPackViewModel.GameServerId });
        }
    }
}